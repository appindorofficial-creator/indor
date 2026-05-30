using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using IndorMvcApp.ViewModels;

namespace IndorMvcApp.Services;

public static class HouseFactDisplayService
{
    private static readonly (string Key, string Title, string Icon, int Order)[] HouseFactSections =
    [
        ("propertyIdentity", "1. Property Identity", "fa-fingerprint", 1),
        ("basicPropertyFacts", "2. Basic Property Facts", "fa-house", 2),
        ("listingMarketData", "3. Listing / Market Data", "fa-tags", 3),
        ("publicRecordsTaxes", "4. Public Records / Taxes", "fa-file-invoice-dollar", 4),
        ("salesHistory", "5. Sales History", "fa-handshake", 5),
        ("mechanicalUtilitySystems", "6. Mechanical / Utility Systems", "fa-fan", 6),
        ("roofExteriorSite", "7. Roof / Exterior / Site", "fa-house-chimney", 7),
        ("foundationStructure", "8. Foundation / Structure", "fa-layer-group", 8),
        ("permitsImprovements", "9. Permits / Improvements", "fa-file-contract", 9),
        ("hoaCommunity", "10. HOA / Community", "fa-people-roof", 10),
        ("schoolsLocationUtilities", "11. Schools / Location / Utilities", "fa-school", 11),
        ("itemsNeedingVerification", "12. Key Items Needing Verification", "fa-clipboard-check", 12),
        ("sources", "13. Sources", "fa-book", 13),
        ("propertyDetails", "Property summary", "fa-chart-pie", 14),
        ("utilityProviders", "Utility providers", "fa-plug", 15)
    ];

    public static HouseFactProfileViewModel BuildProfile(string? rawJson, string? dataSource = null, string? fallbackAddress = null)
    {
        var profile = new HouseFactProfileViewModel
        {
            DataSource = dataSource,
            RawJsonPretty = AttomFieldExtractor.FormatPrettyJson(rawJson),
            HasData = !string.IsNullOrWhiteSpace(rawJson)
        };

        if (string.IsNullOrWhiteSpace(rawJson))
        {
            return profile;
        }

        try
        {
            var root = JsonNode.Parse(rawJson);
            if (root == null)
            {
                return profile;
            }

            profile.FormattedAddress = ReadString(root["formattedAddress"] ?? root["FormattedAddress"]) ?? fallbackAddress;
            profile.Confidence = ReadString(root["confidence"] ?? root["Confidence"]);
            profile.Sections = BuildSections(root);

            profile.FieldCount = profile.Sections.Sum(s =>
                s.SectionKind switch
                {
                    "checklist" or "questions" or "action-flow" => s.ChecklistItems.Count,
                    "sources" => s.Sources.Count,
                    "narrative" => s.Fields.Count + (string.IsNullOrWhiteSpace(s.Paragraph) ? 0 : 1),
                    _ => s.Fields.Count
                });

            HouseFactOverviewBuilder.Apply(profile);
            HouseFactOverviewBuilder.EnsureNearbyPlaceCards(profile);

            return profile;
        }
        catch
        {
            profile.Sections = AttomFieldExtractor.ExtractGroups(rawJson);
            profile.FieldCount = profile.Sections.Sum(s => s.Fields.Count);
            HouseFactOverviewBuilder.Apply(profile);
            HouseFactOverviewBuilder.EnsureNearbyPlaceCards(profile);
            return profile;
        }
    }

    public static List<AttomFieldGroupViewModel> BuildSections(JsonNode root)
    {
        var organized = root["sections"]?.AsArray();
        if (organized != null && organized.Count > 0)
        {
            return BuildOrganizedSections(organized);
        }

        var groups = new List<AttomFieldGroupViewModel>();
        var isHouseFact = root["propertyIdentity"] != null;

        if (isHouseFact)
        {
            foreach (var (key, title, icon, order) in HouseFactSections)
            {
                var node = root[key];
                if (node == null) continue;

                if (key == "itemsNeedingVerification")
                {
                    groups.Add(BuildChecklistSection(title, icon, order, node));
                    continue;
                }

                if (key == "sources")
                {
                    groups.Add(BuildSourcesSection(title, icon, order, node));
                    continue;
                }

                groups.Add(CreateFieldSection(key, title, icon, order, node));
            }

            var confidence = root["confidence"] ?? root["Confidence"];
            if (confidence != null)
            {
                groups.Add(CreateFieldSection("confidence", "Confidence level", "fa-shield-check", 16, confidence));
            }
        }
        else
        {
            groups.AddRange(AttomFieldExtractor.ExtractGroups(root.ToJsonString()));
        }

        return FilterAndSort(groups).Select(EnrichSection).ToList();
    }

    private static List<AttomFieldGroupViewModel> BuildOrganizedSections(JsonArray sections)
    {
        var groups = new List<AttomFieldGroupViewModel>();
        var order = 0;

        foreach (var node in sections)
        {
            if (node is not JsonObject section) continue;
            order++;

            var title = ReadString(section["title"]) ?? $"Section {order}";
            var id = ReadString(section["id"]) ?? title;
            var kind = ReadString(section["kind"]) ?? "fields";
            var icon = IconForSection(id, title);

            var group = new AttomFieldGroupViewModel
            {
                SectionId = Slugify(id),
                Title = title,
                Icon = icon,
                Order = order,
                SectionKind = kind,
                Paragraph = ReadString(section["paragraph"]),
                Notes = ReadString(section["notes"])
            };

            if (section["fields"] is JsonArray fieldsArray)
            {
                foreach (var fieldNode in fieldsArray)
                {
                    if (fieldNode is not JsonObject fieldObj) continue;
                    var label = ReadString(fieldObj["label"]);
                    var value = ReadString(fieldObj["value"]);
                    if (string.IsNullOrWhiteSpace(label) && string.IsNullOrWhiteSpace(value)) continue;

                    group.Fields.Add(new AttomFieldItemViewModel
                    {
                        Label = label ?? "Field",
                        Value = value ?? "—",
                        NeedsVerification = (value ?? "").Contains("verification", StringComparison.OrdinalIgnoreCase)
                            || (value ?? "").Contains("Not publicly confirmed", StringComparison.OrdinalIgnoreCase)
                    });
                }
            }

            if (section["checklistItems"] is JsonArray checklist)
            {
                foreach (var itemNode in checklist)
                {
                    if (itemNode is JsonObject itemObj)
                    {
                        group.ChecklistItems.Add(new HouseFactChecklistItemViewModel
                        {
                            Item = ReadString(itemObj["item"]) ?? ReadString(itemObj["question"]) ?? "—",
                            Status = ReadString(itemObj["status"])
                        });
                    }
                    else
                    {
                        var text = ReadString(itemNode);
                        if (!string.IsNullOrWhiteSpace(text))
                        {
                            group.ChecklistItems.Add(new HouseFactChecklistItemViewModel { Item = text! });
                        }
                    }
                }
            }

            if (section["tableRows"] is JsonArray tableRows)
            {
                var rowIndex = 0;
                foreach (var rowNode in tableRows)
                {
                    rowIndex++;
                    if (rowNode is JsonObject rowObj)
                    {
                        foreach (var kv in rowObj)
                        {
                            if (kv.Value is JsonObject columns)
                            {
                                foreach (var col in columns)
                                {
                                    group.Fields.Add(new AttomFieldItemViewModel
                                    {
                                        Label = $"{Humanize(col.Key)} (row {rowIndex})",
                                        Value = ReadString(col.Value) ?? "—"
                                    });
                                }
                            }
                            else
                            {
                                group.Fields.Add(new AttomFieldItemViewModel
                                {
                                    Label = Humanize(kv.Key),
                                    Value = ReadString(kv.Value) ?? "—"
                                });
                            }
                        }
                    }
                }
            }

            if (group.Fields.Count == 0 && string.IsNullOrWhiteSpace(group.Paragraph)
                && group.ChecklistItems.Count == 0 && string.IsNullOrWhiteSpace(group.Notes))
            {
                continue;
            }

            groups.Add(EnrichSection(group));
        }

        return FilterAndSort(groups);
    }

    private static List<AttomFieldGroupViewModel> FilterAndSort(List<AttomFieldGroupViewModel> groups) =>
        groups
            .Where(g => g.SectionKind is "checklist" or "questions" or "action-flow"
                ? g.ChecklistItems.Count > 0
                : g.SectionKind == "sources"
                    ? g.Sources.Count > 0
                    : g.Fields.Count > 0 || !string.IsNullOrWhiteSpace(g.Paragraph) || !string.IsNullOrWhiteSpace(g.Notes))
            .OrderBy(g => g.Order)
            .ThenBy(g => g.Title)
            .ToList();

    private static string IconForSection(string id, string title)
    {
        var key = id.ToLowerInvariant();
        if (key.Contains("snapshot")) return "fa-table";
        if (key.Contains("identity")) return "fa-fingerprint";
        if (key.Contains("market") || key.Contains("tax") || key.Contains("sales")) return "fa-chart-line";
        if (key.Contains("listing") || key.Contains("features")) return "fa-list-check";
        if (key.Contains("systems") || key.Contains("hvac")) return "fa-fan";
        if (key.Contains("roof") || key.Contains("exterior") || key.Contains("structure")) return "fa-house-chimney";
        if (key.Contains("permit")) return "fa-file-contract";
        if (key.Contains("hoa") || key.Contains("school") || key.Contains("utilit")) return "fa-school";
        if (key.Contains("risk")) return "fa-triangle-exclamation";
        if (key.Contains("missing") || key.Contains("checklist")) return "fa-clipboard-check";
        if (key.Contains("action") || key.Contains("flow")) return "fa-route";
        if (key.Contains("realtor") || key.Contains("question")) return "fa-comments";
        if (key.Contains("summary") || key.Contains("final")) return "fa-flag-checkered";
        if (title.Contains("13.", StringComparison.Ordinal)) return "fa-flag-checkered";
        return "fa-circle-info";
    }

    private static string Humanize(string key)
    {
        if (string.IsNullOrWhiteSpace(key)) return "Field";
        return char.ToUpper(key[0]) + key[1..].Replace('_', ' ');
    }

    private static AttomFieldGroupViewModel CreateFieldSection(string sectionId, string title, string icon, int order, JsonNode node)
    {
        var fields = new List<AttomFieldItemViewModel>();
        AttomFieldExtractor.FlattenFields(node, string.Empty, fields);

        return EnrichSection(new AttomFieldGroupViewModel
        {
            SectionId = Slugify(sectionId),
            Title = title,
            Icon = icon,
            Order = order,
            SectionKind = "fields",
            Fields = fields
                .GroupBy(f => f.Label, StringComparer.OrdinalIgnoreCase)
                .Select(g => g.First())
                .OrderBy(f => f.Label)
                .ToList()
        });
    }

    private static AttomFieldGroupViewModel EnrichSection(AttomFieldGroupViewModel group)
    {
        group.DisplayTitle = CleanTitle(group.Title);
        if (string.IsNullOrWhiteSpace(group.SectionId))
        {
            group.SectionId = Slugify(group.DisplayTitle);
        }

        ExtractStructuredData(group);
        group.ItemCount = CountItems(group);
        group.Summary = BuildSummary(group);
        group.ExpandByDefault = ShouldExpandByDefault(group);
        return group;
    }

    private static void ExtractStructuredData(AttomFieldGroupViewModel group)
    {
        if (!IsSchoolsSection(group)) return;

        var remaining = new List<AttomFieldItemViewModel>();
        foreach (var field in group.Fields)
        {
            if (TryParseSchoolField(field, out var school))
            {
                group.Schools.Add(school);
            }
            else
            {
                remaining.Add(field);
            }
        }

        group.Fields = remaining;
    }

    private static bool IsSchoolsSection(AttomFieldGroupViewModel group)
    {
        var key = $"{group.SectionId} {group.Title} {group.DisplayTitle}".ToLowerInvariant();
        return key.Contains("school") || key.Contains("education") || key.Contains("district");
    }

    private static bool TryParseSchoolField(AttomFieldItemViewModel field, out HouseFactSchoolViewModel school)
    {
        school = new HouseFactSchoolViewModel();
        var label = field.Label.ToLowerInvariant();
        var value = field.Value.Trim();
        if (string.IsNullOrWhiteSpace(value) || value == "—") return false;

        if (!label.Contains("school") && !label.Contains("district") && !label.Contains("elementary")
            && !label.Contains("middle") && !label.Contains("high") && !label.Contains("assigned")
            && !label.Contains("charter") && !label.Contains("magnet"))
        {
            return false;
        }

        school.Name = value;
        school.Detail = field.Label;
        school.Level = DetectSchoolLevel(label, value);
        school.Distance = ExtractDistance(value);
        school.Rating = ExtractRating(label, value);
        return true;
    }

    private static string DetectSchoolLevel(string label, string value)
    {
        var text = $"{label} {value}".ToLowerInvariant();
        if (text.Contains("elementary") || text.Contains("primary")) return "Elementary";
        if (text.Contains("middle") || text.Contains("junior")) return "Middle";
        if (text.Contains("high") || text.Contains("secondary")) return "High";
        if (text.Contains("district")) return "District";
        if (text.Contains("charter")) return "Charter";
        return "School";
    }

    private static string? ExtractDistance(string value)
    {
        var match = Regex.Match(value, @"(\d+(\.\d+)?\s*(mi|miles|km|meters|ft|feet))", RegexOptions.IgnoreCase);
        return match.Success ? match.Value : null;
    }

    private static string? ExtractRating(string label, string value)
    {
        if (label.Contains("rating", StringComparison.OrdinalIgnoreCase)
            || value.Contains('/'))
        {
            return value;
        }

        return null;
    }

    private static int CountItems(AttomFieldGroupViewModel group) =>
        group.SectionKind switch
        {
            "checklist" or "questions" or "action-flow" => group.ChecklistItems.Count,
            "sources" => group.Sources.Count,
            _ => group.Fields.Count + group.Schools.Count + group.Utilities.Count
                + (string.IsNullOrWhiteSpace(group.Paragraph) ? 0 : 1)
        };

    private static string BuildSummary(AttomFieldGroupViewModel group)
    {
        if (!string.IsNullOrWhiteSpace(group.Paragraph))
        {
            return Truncate(group.Paragraph, 140);
        }

        if (group.Schools.Count > 0)
        {
            var names = group.Schools.Take(2).Select(s => s.Name);
            return $"{group.Schools.Count} school(s): {string.Join(", ", names)}";
        }

        var preview = group.Fields
            .Where(f => !f.NeedsVerification && !string.IsNullOrWhiteSpace(f.Value) && f.Value != "—")
            .Take(3)
            .Select(f => $"{f.Label}: {Truncate(f.Value, 36)}");

        var text = string.Join(" · ", preview);
        return string.IsNullOrWhiteSpace(text) ? "Tap to view details" : Truncate(text, 160);
    }

    private static bool ShouldExpandByDefault(AttomFieldGroupViewModel group)
    {
        var key = $"{group.SectionId} {group.DisplayTitle}".ToLowerInvariant();
        return group.Order <= 2
            || key.Contains("snapshot")
            || key.Contains("identity")
            || key.Contains("school")
            || key.Contains("location");
    }

    private static string CleanTitle(string title) =>
        string.IsNullOrWhiteSpace(title)
            ? "Section"
            : Regex.Replace(title.Trim(), @"^\d+\.\s*", string.Empty);

    private static string Slugify(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return "section";
        var slug = Regex.Replace(text.ToLowerInvariant(), @"[^a-z0-9]+", "-").Trim('-');
        return string.IsNullOrWhiteSpace(slug) ? "section" : slug;
    }

    private static string Truncate(string text, int max)
    {
        if (string.IsNullOrWhiteSpace(text) || text.Length <= max) return text;
        return text[..(max - 1)].TrimEnd() + "…";
    }

    private static AttomFieldGroupViewModel BuildChecklistSection(string title, string icon, int order, JsonNode node)
    {
        var items = new List<HouseFactChecklistItemViewModel>();

        if (node is JsonArray array)
        {
            foreach (var item in array)
            {
                var value = ReadString(item);
                if (!string.IsNullOrWhiteSpace(value))
                {
                    items.Add(new HouseFactChecklistItemViewModel { Item = value! });
                }
            }
        }
        else
        {
            var fields = new List<AttomFieldItemViewModel>();
            AttomFieldExtractor.FlattenFields(node, string.Empty, fields);
            items.AddRange(fields
                .Where(f => !string.IsNullOrWhiteSpace(f.Value))
                .Select(f => new HouseFactChecklistItemViewModel { Item = f.Value }));
        }

        return EnrichSection(new AttomFieldGroupViewModel
        {
            SectionId = Slugify(title),
            Title = title,
            Icon = icon,
            Order = order,
            SectionKind = "checklist",
            ChecklistItems = items
        });
    }

    private static AttomFieldGroupViewModel BuildSourcesSection(string title, string icon, int order, JsonNode node)
    {
        var sources = new List<HouseFactSourceViewModel>();

        if (node is JsonArray array)
        {
            foreach (var item in array)
            {
                if (item is not JsonObject obj) continue;

                sources.Add(new HouseFactSourceViewModel
                {
                    SourceName = ReadString(obj["sourceName"] ?? obj["name"]) ?? "Source",
                    Link = ReadString(obj["link"] ?? obj["url"] ?? obj["website"]),
                    InformationFound = ReadString(obj["informationFound"] ?? obj["information"] ?? obj["notes"]) ?? "—",
                    Conflicts = ReadString(obj["conflicts"] ?? obj["conflict"])
                });
            }
        }
        else
        {
            var fields = new List<AttomFieldItemViewModel>();
            AttomFieldExtractor.FlattenFields(node, string.Empty, fields);
            foreach (var field in fields)
            {
                sources.Add(new HouseFactSourceViewModel
                {
                    SourceName = field.Label,
                    InformationFound = field.Value
                });
            }
        }

        return EnrichSection(new AttomFieldGroupViewModel
        {
            SectionId = "sources",
            Title = title,
            Icon = icon,
            Order = order,
            SectionKind = "sources",
            Sources = sources
        });
    }

    private static string? ReadString(JsonNode? node)
    {
        if (node == null) return null;
        return node.GetValueKind() switch
        {
            JsonValueKind.String => node.GetValue<string>(),
            JsonValueKind.Number or JsonValueKind.True or JsonValueKind.False => node.ToJsonString(),
            JsonValueKind.Null => null,
            _ => node.ToString()
        };
    }
}
