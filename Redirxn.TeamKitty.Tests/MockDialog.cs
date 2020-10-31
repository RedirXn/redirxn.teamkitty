using Redirxn.TeamKitty.Services.Application;
using System.Threading.Tasks;

namespace Redirxn.TeamKitty.Tests
{
    public class MockDialog : IDialogService
    {
        string _optionToReturn;
        string _inputToReturn;
        
        public bool SelectOptionCalled { get; private set; }
        public Task Alert(string title, string message, string okButton)
        {
            throw new System.NotImplementedException();
        }

        public async Task<string> GetSingleTextInput(string title, string message)
        {
            return _inputToReturn;
        }
                
        public async Task<string> SelectOption(string title, string cancel, params string[] buttons)
        {
            SelectOptionCalled = true;
            return _optionToReturn;
        }

        public void Make_SelectOptionReturn(string thisOption)
        {            
            _optionToReturn = thisOption;
        }
        public void Make_TextInputReturn(string thisText)
        {
            _inputToReturn = thisText;
        }
    }
}