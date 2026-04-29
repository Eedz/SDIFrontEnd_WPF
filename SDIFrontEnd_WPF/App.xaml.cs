using ITC_Contracts;
using ITCLib;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SDIFrontEnd_WPF.Mappings;
using SDIFrontEnd_WPF.Services;
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
            ServiceProvider serviceProvider;
//#if DEBUG
//            serviceProvider = AddMockServices();

//#else
            serviceProvider = AddServices();
            if (!(await CheckHealth(serviceProvider)))
            {
                MessageBox.Show(
                    "The server is unavailable. The application will now close.",
                    "Server Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                Shutdown();
                return;
            }
            //#endif


            var loader = serviceProvider.GetRequiredService<ReferenceDataService>();
            await loader.LoadAsync();

            var wordingLoader = serviceProvider.GetRequiredService<WordingDataService>();
            await wordingLoader.LoadAsync();


            MainWindow window = new MainWindow();

            var viewModel = new MainWindowViewModel(serviceProvider);
            await viewModel.LoadAsync();

            window.DataContext = viewModel;

            window.Show();
        }

        static async Task<bool> CheckHealth(IServiceProvider serviceProvider)
        {
            try
            {
                var surveyApi = serviceProvider.GetRequiredService<IApiSurveyService>();

                var isHealthy = await surveyApi.GetSurveyByIdAsync(6);
                return isHealthy != null;
            }
            catch (HttpRequestException)
            {
                return false;
            }
            catch (TaskCanceledException)
            {
                return false;
            }
            
                
            //return isHealthy;
        }

        static ServiceProvider AddMockServices()
        {
            IServiceCollection services = new ServiceCollection();

            services.AddSingleton<IFileDialogService, FileDialogService>();
            services.AddSingleton<IDialogService, DialogService>();
            services.AddSingleton<IWindowService, WindowService>();

            AddMockServices(services);

            services.AddLogging(builder =>
            {
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Information);
            });

            // data storage
            services.AddSingleton<ReferenceDataStore>();
            services.AddSingleton<ReferenceDataService>();
            services.AddSingleton<WordingData>();
            services.AddSingleton<WordingDataService>();

            // client services
            services.AddTransient<QuestionImporterService>();
            services.AddSingleton<IMatrixService, MatrixService>();

            AddVMServices(services);

            ServiceProvider serviceProvider = services.BuildServiceProvider();
            return serviceProvider;
        }

        static ServiceProvider AddServices()
        {
            IServiceCollection services = new ServiceCollection();

            // mapping profiles (TODO AddMappers extension)
            services.AddSingleton<IMapperFactory, MapperFactory>();
            services.AddSingleton<IViewModelFactory, ViewModelFactory>();

            services.AddTransient<IMapper<Survey, SurveyDto>, SurveyMapper>();
            services.AddTransient<IMapper<SurveyQuestion, SurveyQuestionDto>, SurveyQuestionMapper>();
            services.AddTransient<IMapper<Wording,WordingDto>, WordingMapper>();
            services.AddTransient<IMapper<ResponseSet, ResponseSetDto>, ResponseSetMapper>();
            services.AddTransient<IMapper<SurveyCohort, SurveyCohortDto>, CohortMapper>();
            services.AddTransient<IMapper<SurveyMode, SurveyModeDto>, ModeMapper>();
            services.AddTransient<IMapper<SurveyUserState, SurveyUserStateDto>, UserStateMapper>();
            services.AddTransient<IMapper<SurveyLanguage, SurveyLanguageDto>, LanguageMapper>();
            services.AddTransient<IMapper<SurveyScreenedProduct, SurveyScreenedProductDto>, ScreenedProductMapper>();
            services.AddTransient<IMapper<PraccingIssue, PraccingIssueDto>, PraccingIssueMapper>();
            services.AddTransient<IMapper<PraccingResponse, PraccingResponseDto>, PraccingResponseMapper>();
            services.AddTransient<IMapper<VariableName, VariableNameDto>, VarNameMapper>();
            services.AddTransient<IMapper<Person, PersonDto>, PersonMapper>();
            services.AddTransient<IMapper<QuestionComment, QuestionCommentDto>, QuestionCommentMapper>();
            services.AddTransient<IMapper<DeletedComment, DeletedCommentDto>, DeletedCommentMapper>();
            services.AddTransient<IMapper<AuditWording, AuditWordingDto>, AuditWordingMapper>();
            services.AddTransient<IMapper<ChangedSurveyQuestion, ChangedSurveyQuestionDto>, ChangedQuestionMapper>();
            

            services.AddSingleton<IFileDialogService, FileDialogService>();
            services.AddSingleton<IDialogService, DialogService>();
            services.AddSingleton<IWindowService, WindowService>();

            AddApiServices(services);

            services.AddLogging(builder =>
            {
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Information);
            });

            // data storage
            services.AddSingleton<ReferenceDataStore>();
            services.AddSingleton<ReferenceDataService>();
            
            services.AddSingleton<WordingData>();
            services.AddSingleton<WordingDataService>();

            // client services
            services.AddTransient<QuestionImporterService>();
            services.AddSingleton<IMatrixService, MatrixService>();

            AddVMServices(services);

            ServiceProvider serviceProvider = services.BuildServiceProvider();
            return serviceProvider;
        }

        private static void AddMockServices(IServiceCollection services)
        {
            services.AddSingleton<IApiSurveyService, MockSurveyService>();
            services.AddSingleton<IApiVarNameService, MockVarNameService>();
            services.AddSingleton<IApiQuestionService, MockQuestionService>();
            services.AddSingleton<IApiReferenceDataService, MockReferenceDataService>();
            services.AddSingleton<IApiWordingService, MockWordingService>();
            services.AddSingleton<IApiUserService, MockUserService>();
            services.AddSingleton<IApiPeopleService, MockPeopleService>();
            services.AddSingleton<IApiCommentService, MockCommentService>();
            services.AddSingleton<IApiPraccingService, MockPraccingService>();
            services.AddSingleton<IApiAuditService, MockAuditService>();
        }

        private static void AddApiServices(IServiceCollection services)
        {
#if RELEASE
            var baseUrl = ConfigurationManager.AppSettings["ApiBaseUrl"]
                    ?? throw new InvalidOperationException("ApiBaseUrl is not configured in App.config");
            #else
            var baseUrl = "https://localhost:7137/";
#endif

            services.AddHttpClient<IApiSurveyService, ApiSurveyService>(client =>
            {
                client.BaseAddress = new Uri(baseUrl);
                client.Timeout = TimeSpan.FromSeconds(30);
            });
            services.AddHttpClient<IApiVarNameService, ApiVarNameService>(client =>
            {
                client.BaseAddress = new Uri(baseUrl);
                client.Timeout = TimeSpan.FromSeconds(30);
            });

            services.AddHttpClient<IApiQuestionService, ApiQuestionService>(client =>
            {
                client.BaseAddress = new Uri(baseUrl);
                client.Timeout = TimeSpan.FromSeconds(30);
            });
            services.AddHttpClient<IApiReferenceDataService, ApiReferenceDataService>(client =>
            {
                client.BaseAddress = new Uri(baseUrl);
                client.Timeout = TimeSpan.FromSeconds(30);
            });

            services.AddHttpClient<IApiWordingService, ApiWordingService>(client =>
            {
                client.BaseAddress = new Uri(baseUrl);
                client.Timeout = TimeSpan.FromSeconds(30);
            });

            services.AddHttpClient<IApiUserService, ApiUserService>(client =>
            {
                client.BaseAddress = new Uri(baseUrl);
                client.Timeout = TimeSpan.FromSeconds(30);
            });

            services.AddHttpClient<IApiPeopleService, ApiPeopleService>(client =>
            {
                client.BaseAddress = new Uri(baseUrl);
                client.Timeout = TimeSpan.FromSeconds(30);
            });

            services.AddHttpClient<IApiCommentService, ApiCommentService>(client =>
            {
                client.BaseAddress = new Uri(baseUrl);
                client.Timeout = TimeSpan.FromSeconds(30);
            });

            services.AddHttpClient<IApiPraccingService, ApiPraccingService>(client =>
            {
                client.BaseAddress = new Uri(baseUrl);
                client.Timeout = TimeSpan.FromSeconds(30);
            });

            services.AddHttpClient<IApiAuditService, ApiAuditService>(client =>
            {
                client.BaseAddress = new Uri(baseUrl);
                client.Timeout = TimeSpan.FromSeconds(30);
            });

            services.AddHttpClient<IApiLabelService, ApiLabelService>(client =>
            {
                client.BaseAddress = new Uri(baseUrl);
                client.Timeout = TimeSpan.FromSeconds(30);
            });
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

            services.AddTransient<VariableInformationViewModel>();
            services.AddTransient<PrefixListViewModel>();
            services.AddTransient<LabelLibraryViewModel>();
            services.AddTransient<TopicContentReportViewModel>();
        }
    }



}
