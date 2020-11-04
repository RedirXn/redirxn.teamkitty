using Redirxn.TeamKitty.Services.Application;
using System;
using System.Threading.Tasks;

namespace Redirxn.TeamKitty.Tests
{
    public class MockDialog : IDialogService
    {
        string _optionToReturn;
        string _inputToReturn;
        string _moneyToReturn;

        public bool SelectOptionCalled { get; private set; }
        public string AlertText { get; private set; }
        public async Task Alert(string title, string message, string okButton)
        {
            AlertText = message;
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

        internal void Make_MoneyInputReturn(string amount)
        {
            _moneyToReturn = amount;
        }

        public async Task<string> GetSingleMoneyInput(string title, string message)
        {
            return _moneyToReturn;
        }
    }
}