using Redirxn.TeamKitty.Services.Application;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Redirxn.TeamKitty.Tests
{
    public class MockDialog : IDialogService
    {
        string _optionToReturn;
        List<string> _inputToReturn = new List<string>();
        string _moneyToReturn;
        List<bool> _confirmResult = new List<bool>();
        int _confirmIterator = 0;
        int _textInputIterator = 0;

        public bool SelectOptionCalled { get; private set; }
        public string AlertText { get; private set; }
        public async Task Alert(string title, string message, string okButton)
        {
            AlertText = message;
        }

        public async Task<string> GetSingleTextInput(string title, string message)
        {
            return _inputToReturn[_textInputIterator++];
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
            _inputToReturn.Add(thisText);
        }

        internal void Make_MoneyInputReturn(string amount)
        {
            _moneyToReturn = amount;
        }

        public async Task<string> GetSingleMoneyInput(string title, string message)
        {
            return _moneyToReturn;
        }

        public async Task<bool> Confirm(string title, string message, string okButton, string cancelButton)
        {
            return _confirmResult[_confirmIterator++];
        }

        internal MockDialog Make_ConfirmReturn(bool v)
        {
            _confirmResult.Add(v);
            return this;
        }
    }
}