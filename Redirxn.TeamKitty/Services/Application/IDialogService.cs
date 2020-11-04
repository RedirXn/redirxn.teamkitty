using System.Threading.Tasks;

namespace Redirxn.TeamKitty.Services.Application
{
    public interface IDialogService
    {
        Task<string> SelectOption(string title, string cancel, params string[] buttons);
        Task<string> GetSingleTextInput(string title, string message);
        Task Alert(string title, string message, string okButton);
        Task<string> GetSingleMoneyInput(string title, string message);
    }
}