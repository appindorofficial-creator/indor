using System.Text.RegularExpressions;
using IndorMvcApp.Models;
using IndorMvcApp.ViewModels;

namespace IndorMvcApp.Services;

public static class SchoolsDisplayService
{
    public static SchoolsFlowViewModel BuildIndex(Propiedad propiedad, PropertyInfoViewModel? info, string? tab = null)
    {
        var context = BuildContext(propiedad, info);
        context.ActiveTab = NormalizeTab(tab);
        return context;
    }

    public static SchoolProfileViewModel? BuildProfile(Propiedad propiedad, PropertyInfoViewModel? info, string schoolId)
    {
        var context = BuildContext(propiedad, info);
        var school = context.AllSchools.FirstOrDefault(s =>
            string.Equals(s.Id, schoolId, StringComparison.OrdinalIgnoreCase));

        if (school == null) return null;

        EnrichProfile(school, context.DistrictName);
        return new SchoolProfileViewModel
        {
            PropiedadId = propiedad.Id,
            Address = context.Address,
            School = school
        };
    }

    public static SchoolsCompareViewModel BuildCompare(Propiedad propiedad, PropertyInfoViewModel? info)
    {
        var context = BuildContext(propiedad, info);
        return new SchoolsCompareViewModel
        {
            PropiedadId = propiedad.Id,
            Address = context.Address,
            DistrictName = context.DistrictName,
            AssignedSchools = context.AssignedSchools,
            QuickTakeaway = BuildQuickTakeaway(context)
        };
    }

    public static SchoolsMapViewModel BuildMap(Propiedad propiedad, PropertyInfoViewModel? info)
    {
        var context = BuildContext(propiedad, info);
        return new SchoolsMapViewModel
        {
            PropiedadId = propiedad.Id,
            Address = context.Address,
            DistrictName = context.DistrictName,
            DistrictNote = context.DistrictNote,
            AssignedSchools = context.AssignedSchools,
            NearbySchools = context.NearbySchools
        };
    }

    private static SchoolsFlowViewModel BuildContext(Propiedad propiedad, PropertyInfoViewModel? info)
    {
        var profile = HouseFactDisplayService.BuildProfile(
            propiedad.AttomRawJson,
            info?.DataSource ?? propiedad.AttomSyncStatus ?? "Estimated",
            propiedad.Direccion ?? info?.FormattedAddress);

        var district = ExtractDistrict(profile, info);
        var schools = ExtractSchools(profile, info, district);
        DedupeAndClassify(schools);

        var assigned = schools.Where(s => s.IsAssigned).OrderBy(SchoolSort).ToList();
        var nearby = schools.Where(s => !s.IsAssigned).OrderBy(SchoolSort).ToList();

        if (assigned.Count == 0 && nearby.Count > 0)
        {
            assigned = nearby.Take(Math.Min(3, nearby.Count)).ToList();
            foreach (var school in assigned) school.IsAssigned = true;
            nearby = nearby.Skip(assigned.Count).ToList();
        }

        return new SchoolsFlowViewModel
        {
            PropiedadId = propiedad.Id,
            Address = propiedad.Direccion ?? info?.FormattedAddress ?? profile.FormattedAddress ?? "Property address",
            DistrictName = district,
            DistrictNote = BuildDistrictNote(district),
            HasData = schools.Count > 0 || !string.IsNullOrWhiteSpace(district),
            AssignedSchools = assigned,
            NearbySchools = nearby,
            AllSchools = schools
        };
    }

    private static List<SchoolItemViewModel> ExtractSchools(
        HouseFactProfileViewModel profile,
        PropertyInfoViewModel? info,
        string? district)
    {
        var schools = new List<SchoolItemViewModel>();
        var schoolSections = profile.Sections
            .Where(s => s.CategoryKey == "schools" || IsSchoolsSection(s))
            .ToList();

        foreach (var section in schoolSections)
        {
            foreach (var item in section.Schools)
            {
                schools.Add(MapSchool(item, district, section.Fields));
            }

            foreach (var field in section.Fields)
            {
                if (TryMapFieldToSchool(field, district, out var school))
                {
                    schools.Add(school);
                }
            }
        }

        if (!string.IsNullOrWhiteSpace(info?.PropertyDetails?.AssignedSchool)
            && !IsUnconfirmed(info.PropertyDetails.AssignedSchool))
        {
            schools.Add(new SchoolItemViewModel
            {
                Id = Slugify(info.PropertyDetails.AssignedSchool),
                Name = info.PropertyDetails.AssignedSchool,
                SchoolType = "Assigned school",
                District = district,
                IsAssigned = true,
                Level = "School",
                Icon = "fa-school",
                Tone = "blue"
            });
        }

        return schools;
    }

    private static SchoolItemViewModel MapSchool(
        HouseFactSchoolViewModel item,
        string? district,
        List<AttomFieldItemViewModel> relatedFields)
    {
        var school = new SchoolItemViewModel
        {
            Id = Slugify(item.Name),
            Name = item.Name,
            Level = item.Level,
            District = district,
            Distance = item.Distance,
            RatingValue = ParseRatingValue(item.Rating),
            RatingLabel = ParseRatingLabel(item.Rating),
            IsAssigned = !item.Detail.Contains("nearby", StringComparison.OrdinalIgnoreCase),
            Icon = IconForLevel(item.Level),
            Tone = ToneForLevel(item.Level)
        };

        ApplyParsedDetails(school, item.Name, item.Detail);
        MergeRelatedFields(school, relatedFields, item.Name);
        return school;
    }

    private static bool TryMapFieldToSchool(AttomFieldItemViewModel field, string? district, out SchoolItemViewModel school)
    {
        school = new SchoolItemViewModel();
        var label = field.Label.ToLowerInvariant();
        var value = field.Value.Trim();
        if (string.IsNullOrWhiteSpace(value) || value == "—" || IsUnconfirmed(value))
        {
            return false;
        }

        if (label.Contains("district") && !label.Contains("school"))
        {
            return false;
        }

        if (!label.Contains("school") && !label.Contains("elementary") && !label.Contains("middle")
            && !label.Contains("high") && !label.Contains("assigned") && !label.Contains("magnet")
            && !label.Contains("charter"))
        {
            return false;
        }

        var level = DetectLevel(label, value);
        school = new SchoolItemViewModel
        {
            Id = Slugify(value),
            Name = CleanSchoolName(value),
            Level = level,
            District = district,
            IsAssigned = label.Contains("assigned") || (!label.Contains("nearby") && level != "District"),
            Icon = IconForLevel(level),
            Tone = ToneForLevel(level)
        };

        ApplyParsedDetails(school, value, field.Label);
        if (label.Contains("rating"))
        {
            school.RatingValue = ParseRatingValue(value);
            school.RatingLabel = ParseRatingLabel(value);
            return false;
        }

        return !string.IsNullOrWhiteSpace(school.Name);
    }

    private static void MergeRelatedFields(SchoolItemViewModel school, List<AttomFieldItemViewModel> fields, string schoolName)
    {
        foreach (var field in fields)
        {
            var label = field.Label.ToLowerInvariant();
            var value = field.Value.Trim();
            if (IsUnconfirmed(value)) continue;

            if (label.Contains("rating") && (label.Contains(SchoolToken(schoolName)) || label.Contains(school.Level.ToLowerInvariant())))
            {
                school.RatingValue ??= ParseRatingValue(value);
                school.RatingLabel ??= ParseRatingLabel(value);
            }
            else if (label.Contains("address") && label.Contains("school"))
            {
                school.Address ??= value;
            }
            else if (label.Contains("phone"))
            {
                school.Phone ??= value;
            }
            else if (label.Contains("website") || label.Contains("url"))
            {
                school.Website ??= value;
            }
            else if (label.Contains("enrollment"))
            {
                school.Enrollment ??= value;
            }
            else if (label.Contains("program") || label.Contains("stem") || label.Contains("arts"))
            {
                school.Programs.Add(new SchoolProgramViewModel
                {
                    Title = field.Label,
                    Description = value,
                    Icon = label.Contains("stem") ? "fa-flask" : label.Contains("art") ? "fa-palette" : "fa-star",
                    Tone = label.Contains("stem") ? "green" : label.Contains("art") ? "purple" : "orange"
                });
            }
        }
    }

    private static void ApplyParsedDetails(SchoolItemViewModel school, string rawValue, string label)
    {
        school.Name = CleanSchoolName(rawValue);
        school.Distance ??= ExtractDistance(rawValue);
        school.CommuteTime ??= ExtractCommute(rawValue);
        school.Grades = ExtractGrades(rawValue) ?? ExtractGrades(label) ?? GradesForLevel(school.Level);
        school.SchoolType = ExtractSchoolType(rawValue, label, school.Level);
        school.RatingValue ??= ParseRatingValue(rawValue);
        school.RatingLabel ??= ParseRatingLabel(rawValue);
    }

    private static void EnrichProfile(SchoolItemViewModel school, string? district)
    {
        school.District ??= district;
        school.AtAGlance =
        [
            new SchoolGlanceItemViewModel { Label = "Grades served", Value = school.Grades, Icon = "fa-graduation-cap" },
            new SchoolGlanceItemViewModel { Label = "School type", Value = school.SchoolType, Icon = "fa-school" },
            new SchoolGlanceItemViewModel { Label = "Distance from home", Value = school.Distance ?? "—", Icon = "fa-location-dot" },
            new SchoolGlanceItemViewModel { Label = "District", Value = school.District ?? "—", Icon = "fa-landmark" },
            new SchoolGlanceItemViewModel { Label = "Enrollment", Value = school.Enrollment ?? "Not publicly confirmed", Icon = "fa-users" }
        ];

        if (school.Programs.Count == 0)
        {
            school.Programs =
            [
                new SchoolProgramViewModel { Title = "Public record data", Description = "Programs were not listed in the saved House Fact profile.", Icon = "fa-circle-info", Tone = "blue" }
            ];
        }
    }

    private static void DedupeAndClassify(List<SchoolItemViewModel> schools)
    {
        var unique = new Dictionary<string, SchoolItemViewModel>(StringComparer.OrdinalIgnoreCase);
        foreach (var school in schools.Where(s => s.Level != "District"))
        {
            var key = Slugify(school.Name);
            if (!unique.TryGetValue(key, out var existing))
            {
                unique[key] = school;
                continue;
            }

            existing.Distance ??= school.Distance;
            existing.RatingValue ??= school.RatingValue;
            existing.RatingLabel ??= school.RatingLabel;
            existing.IsAssigned |= school.IsAssigned;
            existing.Address ??= school.Address;
            existing.Phone ??= school.Phone;
            existing.Website ??= school.Website;
            existing.Enrollment ??= school.Enrollment;
            if (school.Programs.Count > 0) existing.Programs.AddRange(school.Programs);
        }

        schools.Clear();
        schools.AddRange(unique.Values);
    }

    private static string? ExtractDistrict(HouseFactProfileViewModel profile, PropertyInfoViewModel? info)
    {
        if (!string.IsNullOrWhiteSpace(info?.PropertyDetails?.AssignedSchool)
            && info.PropertyDetails.AssignedSchool.Contains("School", StringComparison.OrdinalIgnoreCase))
        {
            // Often formatted as district name in assigned school field for NC
        }

        foreach (var section in profile.Sections)
        {
            foreach (var field in section.Fields)
            {
                if (field.Label.Contains("district", StringComparison.OrdinalIgnoreCase)
                    && !IsUnconfirmed(field.Value))
                {
                    return field.Value.Trim();
                }
            }
        }

        foreach (var section in profile.Sections.Where(IsSchoolsSection))
        {
            var districtSchool = section.Schools.FirstOrDefault(s => s.Level == "District");
            if (districtSchool != null) return districtSchool.Name;
        }

        if (!string.IsNullOrWhiteSpace(info?.PropertyDetails?.CountyName))
        {
            return $"Verify district for {info.PropertyDetails.CountyName} County";
        }

        return null;
    }

    private static string BuildDistrictNote(string? district) =>
        string.IsNullOrWhiteSpace(district)
            ? "District assignment should be verified with the local school district."
            : $"Your home is assigned to {district}. Boundaries may change over time.";

    private static string BuildQuickTakeaway(SchoolsFlowViewModel context)
    {
        if (context.AssignedSchools.Count == 0)
        {
            return "Assigned schools were not fully confirmed in the saved House Fact data. Please verify with your district.";
        }

        var parts = context.AssignedSchools.Select(s => $"{s.Name} ({s.Grades})");
        var district = string.IsNullOrWhiteSpace(context.DistrictName) ? "the local district" : context.DistrictName;
        return $"This property is assigned to {string.Join(", ", parts)} within {district}.";
    }

    private static bool IsSchoolsSection(AttomFieldGroupViewModel section)
    {
        var key = $"{section.SectionId} {section.Title} {section.DisplayTitle}".ToLowerInvariant();
        return key.Contains("school") || key.Contains("education");
    }

    private static string NormalizeTab(string? tab) => tab?.ToLowerInvariant() switch
    {
        "nearby" => "nearby",
        "compare" => "compare",
        "district" => "district",
        _ => "assigned"
    };

    private static int SchoolSort(SchoolItemViewModel s) => s.Level switch
    {
        "Elementary" => 1,
        "Middle" => 2,
        "High" => 3,
        _ => 4
    };

    private static string DetectLevel(string label, string value)
    {
        var text = $"{label} {value}".ToLowerInvariant();
        if (text.Contains("elementary") || text.Contains("primary")) return "Elementary";
        if (text.Contains("middle") || text.Contains("junior")) return "Middle";
        if (text.Contains("high") || text.Contains("secondary")) return "High";
        if (text.Contains("district")) return "District";
        return "School";
    }

    private static string IconForLevel(string level) => level switch
    {
        "Elementary" => "fa-backpack",
        "Middle" => "fa-book-open",
        "High" => "fa-graduation-cap",
        _ => "fa-school"
    };

    private static string ToneForLevel(string level) => level switch
    {
        "Elementary" => "blue",
        "Middle" => "green",
        "High" => "purple",
        _ => "blue"
    };

    private static string GradesForLevel(string level) => level switch
    {
        "Elementary" => "PK - 5",
        "Middle" => "6 - 8",
        "High" => "9 - 12",
        _ => "—"
    };

    private static string ExtractSchoolType(string value, string label, string level)
    {
        var text = $"{value} {label}".ToLowerInvariant();
        if (text.Contains("middle")) return "Middle School";
        if (text.Contains("high")) return "High School";
        if (text.Contains("elementary")) return "Elementary School";
        if (text.Contains("charter")) return "Charter School";
        if (text.Contains("magnet")) return "Magnet School";
        return level == "School" ? "School" : $"{level} School";
    }

    private static string CleanSchoolName(string value)
    {
        var cleaned = Regex.Replace(value, @"\s*[\•\|]\s*.*$", string.Empty);
        cleaned = Regex.Replace(cleaned, @"\(\d+(\.\d+)?\s*(mi|miles|km|min|minutes).*\)", string.Empty, RegexOptions.IgnoreCase);
        return cleaned.Trim();
    }

    private static string? ExtractGrades(string text)
    {
        var match = Regex.Match(text, @"(PK\s*[-–]\s*\d+|K\s*[-–]\s*\d+|\d+\s*[-–]\s*\d+)", RegexOptions.IgnoreCase);
        return match.Success ? match.Value.Replace("–", "-").Replace(" ", "") : null;
    }

    private static string? ExtractDistance(string text)
    {
        var match = Regex.Match(text, @"(\d+(\.\d+)?\s*(mi|miles|km))", RegexOptions.IgnoreCase);
        return match.Success ? match.Value : null;
    }

    private static string? ExtractCommute(string text)
    {
        var match = Regex.Match(text, @"(\d+\s*min(?:utes)?)", RegexOptions.IgnoreCase);
        return match.Success ? match.Value : null;
    }

    private static string? ParseRatingValue(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;
        var match = Regex.Match(text, @"(\d(\.\d)?)");
        return match.Success ? match.Value : null;
    }

    private static string? ParseRatingLabel(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;
        if (text.Contains("great", StringComparison.OrdinalIgnoreCase)) return "Great";
        if (text.Contains("good", StringComparison.OrdinalIgnoreCase)) return "Good";
        if (text.Contains("average", StringComparison.OrdinalIgnoreCase)) return "Average";
        return null;
    }

    private static string Slugify(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return "school";
        var slug = Regex.Replace(text.ToLowerInvariant(), @"[^a-z0-9]+", "-").Trim('-');
        return string.IsNullOrWhiteSpace(slug) ? "school" : slug;
    }

    private static string SchoolToken(string name) =>
        name.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault()?.ToLowerInvariant() ?? string.Empty;

    private static bool IsUnconfirmed(string? value) =>
        string.IsNullOrWhiteSpace(value) || value == "—"
        || value.Contains("not publicly confirmed", StringComparison.OrdinalIgnoreCase)
        || value.Contains("needs verification", StringComparison.OrdinalIgnoreCase)
        || value.Contains("not confirmed", StringComparison.OrdinalIgnoreCase);
}
