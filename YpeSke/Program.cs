using DevExpress.LookAndFeel;
using DevExpress.Skins;
using DevExpress.UserSkins;
using YpeSke.Views;

namespace YpeSke;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        // Apply modern DevExpress skin
        BonusSkins.Register();
        SkinManager.EnableFormSkins();
        UserLookAndFeel.Default.SetSkinStyle(SkinStyle.WXI);

        Application.Run(new MainForm());
    }
}
