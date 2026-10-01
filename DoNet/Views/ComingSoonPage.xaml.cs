using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace DoNet.Views;

/// <summary>
/// Stand-in for shell sections that have no design yet. The section name is
/// passed as the navigation parameter.
/// </summary>
public sealed partial class ComingSoonPage : Page
{
    public ComingSoonPage() => InitializeComponent();

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        var section = e.Parameter as string ?? "Coming soon";
        SectionTitle.Text = section;

        (SectionIcon.Glyph, SectionMessage.Text) = section switch
        {
            "Work" => ("\uE8B7", "Projects, tasks and schedules will live here."),
            "People" => ("\uE716", "Teammates, roles and permissions will live here."),
            "Inbox" => ("\uEA8F", "Alerts and system notifications will land here."),
            _ => ("\uE8B7", "This section hasn't been built yet."),
        };
    }
}
