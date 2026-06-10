namespace IndorMvcApp.ViewModels;

public class ProviderProMessagesInboxViewModel : ProviderProPageBaseViewModel
{
    public int StepNumber { get; set; } = 1;
    public int TotalSteps { get; set; } = 4;
    public string ActiveTab { get; set; } = "all";
    public string? SearchQuery { get; set; }
    public int UnreadCount { get; set; }
    public List<ProviderProMessageThreadViewModel> Threads { get; set; } = [];
    public List<ProviderProFlowStepViewModel> FlowSteps { get; set; } = [];
}

public class ProviderProMessageThreadViewModel
{
    public int ConversationId { get; set; }
    public string CustomerName { get; set; } = "";
    public string Address { get; set; } = "";
    public string Preview { get; set; } = "";
    public string TimeLabel { get; set; } = "";
    public string StatusLabel { get; set; } = "";
    public string StatusClass { get; set; } = "new";
    public string Category { get; set; } = "Job";
    public int UnreadCount { get; set; }
    public string? AvatarUrl { get; set; }
}

public class ProviderProConversationViewModel : ProviderProPageBaseViewModel
{
    public int StepNumber { get; set; } = 2;
    public int TotalSteps { get; set; } = 4;
    public int ConversationId { get; set; }
    public string CustomerName { get; set; } = "";
    public string CustomerInitials { get; set; } = "";
    public bool IsCustomerOnline { get; set; }
    public string? CustomerPhone { get; set; }
    public int? JobId { get; set; }
    public string? JobCode { get; set; }
    public string? JobTitle { get; set; }
    public string? PropertyAddress { get; set; }
    public int? EstimateId { get; set; }
    public string? EstimateCode { get; set; }
    public string? EstimateAmountLabel { get; set; }
    public List<ProviderProChatMessageViewModel> Messages { get; set; } = [];
    public List<ProviderProFlowStepViewModel> FlowSteps { get; set; } = [];
}

public class ProviderProChatMessageViewModel
{
    public string SenderType { get; set; } = "Customer";
    public string Body { get; set; } = "";
    public string TimeLabel { get; set; } = "";
    public bool IsRead { get; set; }
}

public class ProviderProMessageQuickActionsViewModel : ProviderProPageBaseViewModel
{
    public int StepNumber { get; set; } = 3;
    public int TotalSteps { get; set; } = 4;
    public int ConversationId { get; set; }
    public string CustomerName { get; set; } = "";
    public string JobTitle { get; set; } = "";
    public string Address { get; set; } = "";
    public string SelectedAction { get; set; } = "";
    public List<ProviderProMessageQuickActionOptionViewModel> Actions { get; set; } = [];
    public List<ProviderProFlowStepViewModel> FlowSteps { get; set; } = [];
}

public class ProviderProMessageQuickActionOptionViewModel
{
    public string ActionType { get; set; } = "";
    public string Label { get; set; } = "";
    public string Description { get; set; } = "";
    public string IconClass { get; set; } = "fa-paper-plane";
    public string ToneClass { get; set; } = "blue";
}

public class ProviderProMessageSentSuccessViewModel : ProviderProPageBaseViewModel
{
    public int ConversationId { get; set; }
    public int? JobId { get; set; }
    public string CustomerName { get; set; } = "";
    public string JobTitle { get; set; } = "";
    public string SentItemLabel { get; set; } = "";
    public List<ProviderProFlowStepViewModel> FlowSteps { get; set; } = [];
}

public class ProviderProMessageActionDraft
{
    public int ConversationId { get; set; }
    public string ActionType { get; set; } = "";
    public string ActionLabel { get; set; } = "";
}

public class ProviderProSendMessageInput
{
    public int ConversationId { get; set; }
    public string Body { get; set; } = "";
}

public class ProviderProMessageQuickActionInput
{
    public int ConversationId { get; set; }
    public string ActionType { get; set; } = "";
}
