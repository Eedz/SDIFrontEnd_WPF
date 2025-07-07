using ITC_DataAccess;
using ITC_Services;
using System;
using System.Configuration;
using System.Data;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
namespace SDIFrontEnd_WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show("An unhandled exception just occurred: " + e.Exception.Message, "Exception", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            this.DispatcherUnhandledException += Application_DispatcherUnhandledException;


            ServiceProvider serviceProvider = AddServices();
            MainWindow window = new MainWindow();


            // Create the ViewModel to which the main window binds.
            var viewModel = new MainWindowViewModel(serviceProvider);

            // Allow all controls in the window to bind to the ViewModel by setting the 
            // DataContext, which propagates down the element tree.
            window.DataContext = viewModel;

            window.Show();
        }

        static ServiceProvider AddServices()
        {
            IServiceCollection services = new ServiceCollection();
            services.AddSingleton<IFileDialogService, FileDialogService>();
            services.AddSingleton<IDialogService, DialogService>();

#if DEBUG
            services.AddScoped<IDbConnection>(db => new Microsoft.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["ISISConnectionStringTest"].ConnectionString));
#else
            services.AddScoped<IDbConnection>(db => new Microsoft.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["ISISConnectionString"].ConnectionString));
#endif
            services.AddSingleton<LookupProvider>();
            services.AddSingleton<ISurveyRepository, SurveyRepository>();
            services.AddSingleton<IPeopleRepository, PeopleRepository>();
            services.AddSingleton<ICommentRepository, CommentRepository>();
            services.AddSingleton<IVarNameRepository, VarNameRepository>();
            services.AddSingleton<IWordingRepository, WordingRepository>();
            services.AddSingleton<IReferenceDataRepository, ReferenceDataRepository>();
            services.AddSingleton<ISurveyService, SurveyService>();
            services.AddSingleton<IPeopleService, PeopleService>();
            services.AddSingleton<ICommentService, CommentService>();
            services.AddSingleton<IVarNameService, VarNameService>();
            services.AddSingleton<IWordingService, WordingService>();
            services.AddSingleton<IReferenceDataService, ReferenceDataService>();

            ServiceProvider serviceProvider = services.BuildServiceProvider();
            return serviceProvider;
        }
    }



}
