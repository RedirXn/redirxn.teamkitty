using System.Threading.Tasks;
using Xamarin.Forms;

namespace Redirxn.TeamKitty.Services.Application
{
    public class DialogService : IDialogService
    {
        public async Task Alert(string title, string message, string okButton)
        {
            await Xamarin.Forms.Application.Current.MainPage.DisplayAlert(title, message, okButton);
        }
        public async Task<bool> Confirm(string title, string message, string okButton, string cancelButton)
        {
            return await Xamarin.Forms.Application.Current.MainPage.DisplayAlert(title, message, okButton, cancelButton);
        }

        public async Task<string> GetSingleMoneyInput(string title, string message)
        {
            return await Xamarin.Forms.Application.Current.MainPage.DisplayPromptAsync(title, message, keyboard: Keyboard.Numeric);
        }

        public async Task<string> GetSingleTextInput(string title, string message)
        {
            return (await Xamarin.Forms.Application.Current.MainPage.DisplayPromptAsync(title, message, keyboard: Keyboard.Create(KeyboardFlags.CapitalizeWord)))?.Trim();
        }

        public async Task<string> SelectOption(string title, string cancel, params string[] buttons)
        {
            return await Xamarin.Forms.Application.Current.MainPage.DisplayActionSheet(title, cancel, null, buttons);
        }
    }
}