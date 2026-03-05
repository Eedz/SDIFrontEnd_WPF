using ITC_DataAccess;
using ITC_Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SDIFrontEnd_WPF.ViewModels;
using System;
using System.Configuration;
using System.Data;
using System.Net.Http;
using System.Windows;

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

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            this.DispatcherUnhandledException += Application_DispatcherUnhandledException;


            ServiceProvider serviceProvider = AddServices();

            var surveyApi = serviceProvider.GetRequiredService<IApiSurveyService>();

            var isHealthy = await surveyApi.CheckHealthAsync();

            if (!isHealthy)
            {
                MessageBox.Show(
                    "The server is unavailable. The application will now close.",
                    "Server Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                Shutdown();
                return;
            }

            var loader = serviceProvider.GetRequiredService<ReferenceDataService>();

            await loader.LoadAsync();


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
            services.AddSingleton<IWindowService, WindowService>();

            services.AddHttpClient<IApiSurveyService, ApiSurveyService>(client =>
            {
                client.BaseAddress = new Uri("https://localhost:7137/");
                client.Timeout = TimeSpan.FromSeconds(30);
            });
            services.AddHttpClient<IApiVarNameService, ApiVarNameService>(client =>
            {
                client.BaseAddress = new Uri("https://localhost:7137/");
                client.Timeout = TimeSpan.FromSeconds(30);
            });

            services.AddHttpClient<IApiQuestionService, ApiQuestionService>(client =>
            {
                client.BaseAddress = new Uri("https://localhost:7137/");
                client.Timeout = TimeSpan.FromSeconds(30);
            });
            services.AddHttpClient<IApiReferenceDataService, ApiReferenceDataService>(client =>
            {
                client.BaseAddress = new Uri("https://localhost:7137/");
                client.Timeout = TimeSpan.FromSeconds(30);
            });

#if DEBUG
            services.AddScoped<IDbConnection>(db => new Microsoft.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["ISISConnectionStringTest"].ConnectionString));
#else
            services.AddScoped<IDbConnection>(db => new Microsoft.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["ISISConnectionString"].ConnectionString));
#endif
            services.AddLogging(builder =>
            {
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Information);
            });

            services.AddSingleton<ReferenceDataStore>();
            services.AddSingleton<ReferenceDataService>();
            

            services.AddSingleton<IPeopleRepository, PeopleRepository>();
            services.AddSingleton<ICommentRepository, CommentRepository>();
            services.AddSingleton<IVarNameRepository, VarNameRepository>();
            services.AddSingleton<IWordingRepository, WordingRepository>();
            services.AddSingleton<IReferenceDataRepository, ReferenceDataRepository>();
            services.AddSingleton<IUserRepository, UserRepository>();
            services.AddSingleton<IPraccingRepository, PraccingRepository>();
            services.AddSingleton<IAuditRepository, AuditRepository>();

            
            services.AddSingleton<IPeopleService, PeopleService>();
            services.AddSingleton<ICommentService, CommentService>();
            services.AddSingleton<IVarNameService, VarNameService>();
            services.AddSingleton<IWordingService, WordingService>();
       //     services.AddSingleton<IReferenceDataService, ReferenceDataService>();
            services.AddSingleton<IUserService, UserService>();
            services.AddSingleton<IMatrixService, MatrixService>(); 
            services.AddSingleton<IPraccingService, PraccingService>();
            services.AddSingleton<IAuditService, AuditService>();

            services.AddTransient<QuestionImporterService>();

            AddVMServices(services);


            services.AddHttpClient<ApiSurveyService>(client =>
            {
                client.BaseAddress = new Uri("https://localhost:5001/"); // your API URL
            });

            ServiceProvider serviceProvider = services.BuildServiceProvider();
            return serviceProvider;
        }

        private static void AddVMServices(IServiceCollection services)
        {
            services.AddTransient<MainWindowViewModel>();

            services.AddTransient<HomeViewModel>();

            services.AddTransient<SurveyManagerViewModel>();

            services.AddTransient<PraccingEntryViewModel>();
            services.AddTransient<PraccingReportViewModel>();
            services.AddTransient<PraccingImportViewModel>();
            services.AddTransient<PraccingSheetViewModel>();

            services.AddTransient<RenameVarsViewModel>();
            services.AddTransient<VarNameChangeViewModel>();
            services.AddTransient<QuestionHistoryManagerViewModel>();
            
            services.AddTransient<QuestionImporterViewModel>();

            services.AddTransient<QuestionSearchViewModel>();
            services.AddTransient<HarmonyReportViewModel>();
            services.AddTransient<QuestionSurveyMatrixViewModel>();
        }
    }



}
