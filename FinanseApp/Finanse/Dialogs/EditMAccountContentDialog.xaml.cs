using Finanse.DataAccessLayer;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Finanse.Models;
using Finanse.Models.MAccounts;

namespace Finanse.Dialogs {

    public sealed partial class EditMAccountContentDialog : INotifyPropertyChanged {

       // private readonly int MaxLength = NewOperation.MaxLength;

        private readonly TextBoxEvents _textBoxEvents = new TextBoxEvents();

        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged(string propertyName) {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    

        private readonly List<KeyValuePair<object, object>> _colorBase = ((ResourceDictionary) Application.Current.Resources["ColorBase"]).ToList();
        
        private string ColorKey => SelectedColor.Key.ToString();

        private KeyValuePair<object, object> _selectedColor;
        public KeyValuePair<object, object> SelectedColor {
            get {
                if (_selectedColor.Key == null || _selectedColor.Value == null)
                    _selectedColor = _colorBase.ElementAt(3);

                return _selectedColor;
            }
            set {
                _selectedColor = value;
                //RaisePropertyChanged(nameof(Color));
                RaisePropertyChanged(nameof(PrimaryButtonEnabling));
            }
        }

        private MAccount _selectedBankAccount;
        public MAccount SelectedBankAccount {
            get => _selectedBankAccount;
            set {
                if (value == null || value.Id == -1) {
                    _selectedBankAccount = null;
                 //   BankAccountsComboBox.SelectedIndex = -1;
                }
                else
                    _selectedBankAccount = value;
                RaisePropertyChanged(nameof(PrimaryButtonEnabling));
            }
        }

        private readonly IEnumerable<MAccount> _bankAccounts =
            new List<MAccount> {new MAccount {Name = "Brak", Id = -1}}.AsEnumerable().Concat(MAccountsDal.GetAllAccounts());


        private bool PrimaryButtonEnabling {
            get {
                if (_accountToEdit.Equals(EditedAccount))
                    return false;
                if (string.IsNullOrWhiteSpace(NameValue.Text))
                    return false;
                if (MAccountsDal.AccountExistInBaseByName(NameValue.Text) && NameValue.Text.Trim().ToLower() != _accountToEdit.Name.Trim().ToLower())
                    return false;

                return true;
            }
        }

        public MAccount EditedAccount {
            get {
                MAccount newAccount;

                if (SelectedBankAccount != null)
                    newAccount = new SubMAccount {
                        BossAccountGlobalId = SelectedBankAccount.GlobalId
                    };
                else {
                    newAccount = new MAccount();
                }

                newAccount.Name = NameValue.Text;
                newAccount.ColorKey = ColorKey;

                return newAccount;
            }
        }

        private readonly MAccount _accountToEdit;

        public EditMAccountContentDialog(MAccount accountToEdit) {
            InitializeComponent();

            _accountToEdit = accountToEdit.Clone();

            NameValue.Text = accountToEdit.Name;
            SelectedColor = _colorBase.SingleOrDefault(i => i.Key.ToString() == accountToEdit.ColorKey);
            if (accountToEdit is SubMAccount)
                SelectedBankAccount = _bankAccounts.FirstOrDefault(i => i.GlobalId == ((SubMAccount)accountToEdit).BossAccountGlobalId);
            else
                _bankAccounts = _bankAccounts.Where(i => i.GlobalId != accountToEdit.GlobalId);
        }

        private void NewCategory_AddButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args) {
            MAccountsDal.UpdateAccount(EditedAccount);
        }
        
        public Brush NameValueForeground => !IsPrimaryButtonEnabled && _accountToEdit.Name.Trim().ToLower() != NameValue.Text.Trim().ToLower()
            ? (SolidColorBrush)Application.Current.Resources["RedColorStyle"]
            : (SolidColorBrush)Application.Current.Resources["Text"];

        private void NameValue_OnTextChanged(object sender, TextChangedEventArgs e) {
            RaisePropertyChanged(nameof(PrimaryButtonEnabling));
            NameValue.Foreground = NameValueForeground;
        }
    }
}
