using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DurableLoans.LoanOffice.ToBeApproved.Wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IDisposable
    {
        public MainWindow()
        {
            InitializeComponent();

            HttpClient = new HttpClient();
            ApiClient = new DurableLoans.LoanOffice.ToBeApproved.Wpf.swagger_v3Client(HttpClient);
        }

        public HttpClient HttpClient { get; }
        public swagger_v3Client ApiClient { get; }
        public IEnumerable<LoanApplicationResultRecord> Loans { get; private set; }

        public void Dispose()
        {
            if (HttpClient != null) HttpClient.Dispose();
        }

        internal void LoadIncomingLoans()
        {
            Application.Current.Dispatcher.Invoke(new Action(async () => 
            {
                _btnToPush.IsEnabled = false;

                ApiClient.BaseUrl = "http://durableloans-inbox.azurewebsites.net/";
                Loans = await ApiClient.ToBeApprovedAsync();

                /*_dataGrid01.ItemsSource = Loans.Select(x =>
                {
                    return new
                    {
                        LoanId = x.Id,
                        FirstName = x.LoanApplication.Application.Applicant.FirstName,
                        LastName = x.LoanApplication.Application.Applicant.LastName,
                        LoanAmount = x.LoanApplication.Application.LoanAmount.Amount,
                        AgencyResults = x.LoanApplication.AgencyResults
                    };
                });*/

                _dataGrid01.ItemsSource = Loans;

                _btnToPush.IsEnabled = true;
            }));
        }

        protected override void OnActivated(EventArgs e)
        {
            Task.Run(LoadIncomingLoans);
            base.OnActivated(e);
        }

        private void OnLoanDetailButtonClicked(object sender, RoutedEventArgs e)
        {
            var selectedLoan = ((Button)sender).DataContext as LoanApplicationResultRecord;
            if(selectedLoan != null)
            {
                Debug.WriteLine(selectedLoan.Id);
            }
        }

        private void OnGetLoansClicked(object sender, RoutedEventArgs e)
        {
            Task.Run(LoadIncomingLoans);
        }
    }
}
