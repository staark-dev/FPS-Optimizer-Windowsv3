using FPSOptimizer.Localization;

namespace FPSOptimizer.UI.Pages;

public partial class PageWelcome : System.Windows.Controls.Page
{
    public PageWelcome()
    {
        InitializeComponent();
        UpdateLanguage();
    }

    public void UpdateLanguage()
    {
        TxtWelcomeTo.Text   = StringResources.Get("Welcome.WelcomeTo");
        TxtAppName.Text     = StringResources.Get("Welcome.AppName");
        TxtSubtitle.Text    = StringResources.Get("Welcome.Subtitle");
        TxtFeat1Title.Text  = StringResources.Get("Welcome.Safe");
        TxtFeat1Desc.Text   = StringResources.Get("Welcome.SafeDesc");
        TxtFeat2Title.Text  = StringResources.Get("Welcome.Perms");
        TxtFeat2Desc.Text   = StringResources.Get("Welcome.PermsDesc");
        TxtFeat3Title.Text  = StringResources.Get("Welcome.FPS");
        TxtFeat3Desc.Text   = StringResources.Get("Welcome.FPSDesc");
        TxtFeat4Title.Text  = StringResources.Get("Welcome.Wizard");
        TxtFeat4Desc.Text   = StringResources.Get("Welcome.WizardDesc");
        TxtFeat5Title.Text  = StringResources.Get("Welcome.NoBloat");
        TxtFeat5Desc.Text   = StringResources.Get("Welcome.NoBloatDesc");
        TxtFeat6Title.Text  = StringResources.Get("Welcome.Restore");
        TxtFeat6Desc.Text   = StringResources.Get("Welcome.RestoreDesc");
        TxtWarnTitle.Text   = StringResources.Get("Welcome.WarnTitle");
        TxtWarn1.Text       = StringResources.Get("Welcome.Warn1");
        TxtWarn2.Text       = StringResources.Get("Welcome.Warn2");
        TxtWarn3.Text       = StringResources.Get("Welcome.Warn3");
        TxtWarn4.Text       = StringResources.Get("Welcome.Warn4");
        TxtWarn5.Text       = StringResources.Get("Welcome.Warn5");
    }
}
