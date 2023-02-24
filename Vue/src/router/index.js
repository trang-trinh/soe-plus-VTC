import { calendarFormat } from "moment/moment";
import { createRouter, createWebHistory } from "vue-router";
import { store } from "../store/store";
const AppView = () =>
    import ("../App.vue");
const HomeView = () =>
    import ("../views/public/home/dashboard.vue");
const ModulesView = () =>
    import ("../views/hethong/ModulesView.vue");
const UserView = () =>
    import ("../views/hethong/UserView.vue");
const RolesView = () =>
    import ("../views/hethong/RolesView.vue");
const WebAcessView = () =>
    import ("../views/hethong/WebAcessView.vue");
const LogsView = () =>
    import ("../views/hethong/LogsView.vue");
const TestCaseView = () =>
    import ("../views/hethong/TestCaseView.vue");
const ConfigView = () =>
    import ("../views/hethong/ConfigView.vue");
const SQLView = () =>
    import ("../views/hethong/SQLView.vue");
const DonviView = () =>
    import ("../views/hethong/DonviView.vue");
const GroupTypeView = () =>
    import ("../views/hethong/GroupTypeView.vue");
const GroupView = () =>
    import ("../views/hethong/GroupView.vue");
const ConfigDonviView = () =>
    import ("../views/hethong/ConfigDonviView.vue");
const LoginView = () =>
    import ("../views/LoginView.vue");
const Error = () =>
    import ("../error/404.vue");
//Scraper
// const ScraperView = () =>
//     import ("../views/scraper/ScraperView.vue");

//Folder
const FolderView = () =>
    import ("../views/folder/FolderView.vue");
const UploadView = () =>
    import ("../views/folder/UploadView.vue");
//Options setting account
const OptionsAccount = () =>
    import ("../views/options/OptionsAccount.vue");
// Data using organization
const DataUsing = () =>
    import ("../views/hethong/DataUsing.vue");
//Canva
const SQLTableView = () =>
    import ("../views/autocode/SQLTableView.vue");
//Danh mục
//Sys
//CMS
const CMSView = () =>
    import ("../views/cms/CMSView.vue");
const CMSHomeView = () =>
    import ("../views/cms/CMSHomeView.vue");
const LangView = () =>
    import ("../views/cms/LangView.vue");
const CMSLogsView = () =>
    import ("../views/cms/LogsView.vue");
const NewsView = () =>
    import ("../views/cms/NewsView.vue");
const SlideShowView = () =>
    import ("../views/cms/SlideShowView.vue");
const TopicFlagView = () =>
    import ("../views/cms/TopicFlagView.vue");
const TopicView = () =>
    import ("../views/cms/TopicView.vue");
//Canva
const CanvaView = () =>
    import ("../views/canva/CanvaView.vue");
//API
// const Project = () =>
//     import ("../views/project/Project.vue");
// const Api = () =>
//     import ("../views/project/Api.vue");
// const Table = () =>
//     import ("../views/project/Table.vue");
// const Plugin = () =>
//     import ("../views/project/Plugin.vue");
//TUDIEN
const BrowseRole = () =>
    import ("../views/dictionary/NhomDuyet.vue");
const DocRole = () =>
    import ("../views/dictionary/NhomChucNang.vue");
const Gif = () =>
    import ("../views/dictionary/Gif.vue");
const Places = () =>
    import ("../views/dictionary/Places.vue");
const Positions = () =>
    import ("../views/dictionary/Positions.vue");
const Dispatch = () =>
    import ("../views/dictionary/Dispatch.vue");
const Email = () =>
    import ("../views/dictionary/Email.vue");
const Cagroup = () =>
    import ("../views/dictionary/Cagroup.vue");
const Field = () =>
    import ("../views/dictionary/Field.vue");
const IssuePlace = () =>
    import ("../views/dictionary/IssuePlace.vue");
const RecevePlace = () =>
    import ("../views/dictionary/RecevePlace.vue");
const Security = () =>
    import ("../views/dictionary/Security.vue");
const SendWay = () =>
    import ("../views/dictionary/SendWay.vue");
const Signer = () =>
    import ("../views/dictionary/Signer.vue");
const Urgency = () =>
    import ("../views/dictionary/Urgency.vue");
const CaPosition = () =>
    import ("../views/dictionary/CaPosition.vue");
const Tem = () =>
    import ("../views/dictionary/Tem.vue");
const Emote = () =>
    import ("../views/dictionary/Emotes.vue");
const Tags = () =>
    import ("../views/dictionary/Tags.vue");
const Type = () =>
    import ("../views/dictionary/Type.vue");
const Status = () =>
    import ("../views/dictionary/Status.vue");
const Project = () =>
    import ("../views/project/Project.vue");
const Api = () =>
    import ("../views/project/Api.vue");
const Table = () =>
    import ("../views/project/Table.vue");
const Plugin = () =>
    import ("../views/project/Plugin.vue");
const Task = () =>
    import ("../views/task/Task.vue");
const TaskGroup = () =>
    import ("../views/task/TaskGroup.vue");
const TaskCheck = () =>
    import ("../views/task/TaskCheck.vue");
const CheckListReport = () =>
    import ("../views/task/CheckListReport.vue");
const MainReport = () =>
    import ("../views/task/MainReport.vue");
const Organization = () =>
    import ("../views/hethong/Organization.vue");
const BrowserByDepartment = () =>
    import ("../views/doc/Config/BrowserByDepartment.vue");
//Video
const VideosMain = () =>
    import ("../views/videos/VideosMain.vue");
const VideoDetails = () =>
    import ("../views/videos/VideoDetails.vue");
const VideoSearch = () =>
    import ("../views/videos/VideoSearch.vue");
const VideosView = () =>
    import ("../views/videos/VideosView.vue");

//  Tin tức
const NewsMain = () =>
    import ("../views/news/NewsMain.vue");
const DirectOperator = () =>
    import ("../views/news/DirectOperator.vue");
const NewDetails = () =>
    import ("../views/news/NewDetails.vue");
const NewsKeyWords = () =>
    import ("../views/news/NewsKeyWords.vue");
const ReportConfig = () =>
    import ("../views/report/ReportConfig.vue");

// Birthday
const BirthDay = () =>
    import ("../views/birthday/birthday.vue");
const SendCongs = () =>
    import ("../views/birthday/SendCongrations.vue");

// Doc
const DocReceive_Detail = () =>
    import ("../views/doc/DocReceive.vue");
const DocReceive = () =>
    import ("../views/doc/DocReceive.vue");
const DocSend = () =>
    import ("../views/doc/DocSend.vue");
const DocStore = () =>
    import ("../views/doc/DocStore.vue");
const DocReceiveReport = () =>
    import ("../views/doc/DocReceiveReport.vue");
const DocSendEmailReport = () =>
    import ("../views/doc/DocSendEmailReport.vue");
const DocInternalReport = () =>
    import ("../views/doc/DocInternalReport.vue");
const DocSendReport = () =>
    import ("../views/doc/DocSendReport.vue");
const DocConfigNumber = () =>
    import ("../views/doc/DocConfigNumber.vue");
const DocAxisConfig = () =>
    import ("../views/doc/Config/DocAxisConfig.vue");
const DocReservationNumber = () =>
    import ("../views/doc/DocReservationNumber.vue");
//Calendar
const calendarMain = () =>
    import ("../views/calendar/calendarmain.vue");
const calendarDetail = () =>
    import ("../views/calendar/calendarweek/calendarweekdetail.vue");
const calendarReserve = () =>
    import ("../views/calendar/calendarweek/calendarreserve.vue");
const calendarPendding = () =>
    import ("../views/calendar/calendarweek/calendarpendding.vue");
const calendarFollow = () =>
    import ("../views/calendar/calendarweek/calendarfollow.vue");
const calendarEnact = () =>
    import ("../views/calendar/calendarweek/calendarenact.vue");
const calendarPlantripDetail = () =>
    import ("../views/calendar/calendarplantrip/calendarplantripdetail.vue");
const calendarPlantripReserve = () =>
    import ("../views/calendar/calendarplantrip/calendarreserve.vue");
const calendarPlantripPendding = () =>
    import ("../views/calendar/calendarplantrip/calendarpendding.vue");
const calendarPlantripFollow = () =>
    import ("../views/calendar/calendarplantrip/calendarfollow.vue");
const calendarPlantripEnact = () =>
    import ("../views/calendar/calendarplantrip/calendarenact.vue");
const calendarDutyReserve = () =>
    import ("../views/calendar/calendarduty/calendardutyreserve.vue");
const calendarDutyPendding = () =>
    import ("../views/calendar/calendarduty/calendardutypendding.vue");
const calendarDutyFollow = () =>
    import ("../views/calendar/calendarduty/calendardutyfollow.vue");
const calendarDutyApproved = () =>
    import ("../views/calendar/calendarduty/calendardutyapproved.vue");
const calendarBoardroom = () =>
    import ("../views/calendar/category/boardroom.vue");
const calendarCar = () =>
    import ("../views/calendar/category/car.vue");
const calendarPosition = () =>
    import ("../views/calendar/category/position.vue");
const calendarConfig = () =>
    import ("../views/calendar/config/calendarconfig.vue");
const calendarDutyConfig = () =>
    import ("../views/calendar/config/calendardutyconfig.vue");
// Thống kê số liệu
const statisticalMain = () =>
    import ("../views/statistical/statisticalmain.vue");
const statisticalConfig = () =>
    import ("../views/statistical/statisticalconfig.vue");
const statisticalChart = () =>
    import ("../views/statistical/statisticalchart.vue");
// Luật Việt Nam
const LawMain = () =>
    import ("../views/law/LawMain.vue");
//Đặt cơm
const BookingMeal = () =>
    import ("../views/bookingmeal/BookingMeal.vue");
const BookingMealDetail = () =>
    import ("../views/bookingmeal/BookingMeal.vue");
const UserBooking = () =>
    import ("../views/bookingmeal/UserBooking.vue");
const WeeksReport = () =>
    import ("../views/bookingmeal/WeeksReport.vue");
const UsersReport = () =>
    import ("../views/bookingmeal/UsersReport.vue");
const BookingConfig = () =>
    import ("../views/bookingmeal/BookingConfig.vue");
const BookingDaily = () =>
    import ("../views/bookingmeal/BookingDaily.vue");
// Danh mục Luật
// Law - Ngôn ngữ
const LawDocLanguage = () =>
    import ("../views/law/LawDocLanguage.vue");
// Law - Loại văn bản
const LawDocTypes = () =>
    import ("../views/law/LawDocTypes.vue");
// Law - Cơ quan ban hành
const LawDocIssuePlaces = () =>
    import ("../views/law/LawDocIssuePlaces.vue");
// Law - Lĩnh vực hoạt động
const LawDocFields = () =>
    import ("../views/law/LawDocFields.vue");
// Law - Người ký
const LawDocSigners = () =>
    import ("../views/law/LawDocSigners.vue");
//Trình diễn
const ShowsMain = () =>
    import ("../views/shows/ShowsMain.vue");
const Shows = () =>
    import ("../views/shows/Shows.vue");
const ShowDetails = () =>
    import ("../views/shows/ShowDetails.vue");
//------ Tài sản-----
//Từ điển tài sản
const DeviceMain = () =>
    import ("../views/device/DeviceMain.vue");
// Thẻ tài sản
const DeviceCard = () =>
    import ("../views/device/DeviceCard.vue");
// Nhà cung cấp
const DeviceProvider = () =>
    import ("../views/device/DeviceProvider.vue");
//Loại tài sản
const DeviceType = () =>
    import ("../views/device/DeviceType.vue");
const DeviceGroups = () =>
    import ("../views/device/DeviceGroups.vue");
const AcceptHandover = () =>
    import ("../views/device/AcceptHandover.vue");
const DeviceExpiration = () =>
    import ("../views/device/report/DeviceExpiration.vue");
const DeviceInsurance = () =>
    import ("../views/device/report/DeviceInsurance.vue");
const DeviceInventoryReport = () =>
    import ("../views/device/report/DeviceInventoryReport.vue");
const DeviceFollows = () =>
    import ("../views/device/DeviceFollows.vue");
const DeviceApproved = () =>
    import ("../views/device/DeviceApproved.vue");
// Đơn vị tính
const DeviceUnit = () =>
    import ("../views/device/DeviceUnit.vue");
// Kho
const DeviceWareHouse = () =>
    import ("../views/device/DeviceWareHouse.vue");
const ConfigDeHandover = () =>
    import ("../views/device/ConfigDeHandover.vue");
const MyAssets = () =>
    import ("../views/device/MyAssets.vue");

// Cấu hình
const DeviceRepair = () =>
    import ("../views/device/DeviceRepair.vue");
const DeviceInventory = () =>
    import ("../views/device/DeviceInventory.vue");
const DeviceAcceptInventory = () =>
    import ("../views/device/DeviceAcceptInventory.vue");
const DeviceRecall = () =>
    import ("../views/device/DeviceRecall.vue");

const ConfigNumber = () =>
    import ("../views/device/ConfigNumber.vue");
const ConfigGroups = () =>
    import ("../views/device/ConfigGroups.vue");
const Handover = () =>
    import ("../views/device/DeviceHandover.vue");
const DeviceLog = () =>
    import ("../views/device/DeviceLog.vue");

const DeviceManufacturer = () =>
    import ("../views/device/DeviceManufacturer.vue");
const DeviceStatus = () =>
    import ("../views/device/DeviceStatus.vue");
// Chat
const Chat_Message = () =>
    import ("../views/chat/Chat_Message.vue");
const Chat_Contact = () =>
    import ("../views/chat/Chat_Contact.vue");
const Chat_Detail = () =>
    import ("../views/chat/Chat_Message.vue");
// Tráng
// Nhóm dự án
const ProjectMain = () =>
    import ("../views/project_main/ProjectMain.vue");
const ProjectGroup = () =>
    import ("../views/task_ca/ProjectGroup.vue");
const TaskGroups = () =>
    import ("../views/task_ca/taskgroups.vue");
const TaskOrigin = () =>
    import ("../views/task_origin/TaskOrigin.vue");
const TaskOriginDetail = () =>
    import ("../views/task_origin/TaskOrigin.vue");
const taskWeights = () =>
    import ("../views/tasks/config/taskWeights.vue");
const BrowseGroup = () =>
    import ("../views/tasks/config/BrowseGroup.vue");
const TaskDepartmentConfiguration = () =>
    import ("../views/department_configuration/TaskDepartmentConfiguration.vue");
const TaskReportPersonal = () =>
    import ("../views/task_report/TaskReportPersonal.vue");
const TaskReportDepartment = () =>
    import ("../views/task_report/TaskReportDepartment.vue");
const TaskReviewReport = () =>
    import ("../views/tasks/task_report_person/reviewReport.vue");
const TaskPersonCreateReport = () =>
    import ("../views/tasks/task_report_person/createReport.vue");
const TaskPersonConfig = () =>
    import ("../views/tasks/config/TaskPersonConfig.vue");
//hrm
// const Interview = () =>
//     import ("../views/hrm/category/Interview.vue");
const Hrm_Info = () =>
    import ("../views/hrm/Hrm_Info.vue");
const HrmProfile = () =>
    import ("../views/hrm/profile/profile.vue");
const HrmProfileInfo = () =>
    import ("../views/hrm/profile/component/profileinfo.vue");
const HrmContract = () =>
    import ("../views/hrm/contract/contract.vue");
const Insurance = () =>
    import ("../views/hrm/insurance/insurance.vue");
const Hrm_File = () =>
    import ("../views/hrm/files/hrm_file.vue");
const Hrm_campaign = () =>
    import ("../views/hrm/recruitment/hrm_campaign.vue");
const Hrm_paycheck = () =>
    import ("../views/hrm/declare/hrm_paycheck.vue");

//end
// TV
const ConfigScreenTV = () =>
    import ("../views/tivi/tivi_screen_config.vue");
const ScreenTV = () =>
    import ("../views/tivi/tivi_screen.vue");
// Tài liệu
const FileMain_Detail = () =>
    import ("../views/files/FileMain.vue");
const FileFolder = () =>
    import ("../views/files/FileFolder.vue");
const FileMain = () =>
    import ("../views/files/FileMain.vue");
const FileLog = () =>
    import ("../views/files/FileLog.vue");
const FileLogViews = () =>
    import ("../views/files/FileLogViews.vue");
const FileLogUsers = () =>
    import ("../views/files/FileLogUsers.vue");
const FileConfig = () =>
    import ("../views/files/FileConfig.vue");
const totalReport = () =>
    import ("../views/tasks/task_report_person/totalReport.vue");
const TaskDashboard = () =>
    import ("../views/tasks/dashboard/DashboardMain.vue");
const SQLDB_Query = () =>
    import ("../views/sql_query/sql_query.vue");
// HRM_ca
const AcademicLevel = () =>
    import ("../views/hrm/category/caAcademicLevel.vue");
const caHRMBank = () =>
    import ("../views/hrm/category/caBank.vue");
const caHRMCertificate = () =>
    import ("../views/hrm/category/caCertificate.vue");
const caHRMClassification = () =>
    import ("../views/hrm/category/caClassification.vue");
const caHRMCulturalLevel = () =>
    import ("../views/hrm/category/caCulturalLevel.vue");
const caHRMDegree = () =>
    import ("../views/hrm/category/caDegree.vue");
const caHRMEthnic = () =>
    import ("../views/hrm/category/caEthnic.vue");
const caHRMEvaluationCriteria = () =>
    import ("../views/hrm/category/caEvaluationCriteria.vue");
const caHRMFaculty = () =>
    import ("../views/hrm/category/caFaculty.vue");
const caHRMFormality = () =>
    import ("../views/hrm/category/caFormality.vue");
const caHRMFormTraining = () =>
    import ("../views/hrm/category/caFormTraining.vue");
const caHRMIdentityPapers = () =>
    import ("../views/hrm/category/caIdentityPapers.vue");
const caHRMIdentityPlace = () =>
    import ("../views/hrm/category/caIdentityPlace.vue");
const caHRMInformaticLevel = () =>
    import ("../views/hrm/category/caInformaticLevel.vue");
const caHRMInterviewRound = () =>
    import ("../views/hrm/category/caInterviewRound.vue");
const caHRMLanguageLevel = () =>
    import ("../views/hrm/category/caLanguageLevel.vue");
const caHRMLearningPlace = () =>
    import ("../views/hrm/category/caLearningPlace.vue");
const caHRMManagementMajor = () =>
    import ("../views/hrm/category/caManagementMajor.vue");
const caHRMManagementState = () =>
    import ("../views/hrm/category/caManagementState.vue");
const caHRMNationality = () =>
    import ("../views/hrm/category/caNationality.vue");
const caHRMPersonnelLevel = () =>
    import ("../views/hrm/category/caPersonnelLevel.vue");
const caHRMPoliticalTheory = () =>
    import ("../views/hrm/category/caPoliticalTheory.vue");
const caHRMProfessionalWork = () =>
    import ("../views/hrm/category/caProfessionalWork.vue");
const caHRMRelationship = () =>
    import ("../views/hrm/category/caRelationship.vue");
const caHRMReligion = () =>
    import ("../views/hrm/category/caReligion.vue");
const caHRMSpecialization = () =>
    import ("../views/hrm/category/caSpecialization.vue");
const caHRMTitle = () =>
    import ("../views/hrm/category/caTitle.vue");
const caHRMTypeContract = () =>
    import ("../views/hrm/category/caTypeContract.vue");
const caHRMVacancy = () =>
    import ("../views/hrm/category/caVacancy.vue");
const caHRMWage = () =>
    import ("../views/hrm/category/caWage.vue");
const caLeavingReason = () =>
    import ("../views/hrm/category/caLeavingReason.vue");
    const caExperience = () =>
    import ("../views/hrm/category/caExperience.vue");
    const caCandidate = () =>
    import ("../views/hrm/category/caCandidate.vue");
    
const caClassroom = () =>
    import ("../views/hrm/category/caClassroom.vue");
const caReceipt = () =>
    import ("../views/hrm/category/caReceipt.vue");
const caEnectingGroup = () =>
    import ("../views/hrm/category/caEnectingGroup.vue");

// const FileMain_Detail = () =>
//     import ("../views/files/FileMain.vue");

///HRM
const HRM_Training = () =>
    import ("../views/hrm/training/hrm_training.vue");

// Request
const Request_Dashboard = () => import("../views/request/request_dashboard.vue");
const Request_Request = () => import("../views/request/request.vue");
const Request_Team = () => import("../views/request/request_team.vue");
const Request_Document = () => import("../views/request/request_document.vue");
const Request_Ca_Group = () => import("../views/request/category/ca_group.vue");
const Request_Ca_GroupTeam = () => import("../views/request/category/ca_groupteam.vue");
const Request_Ca_Team = () => import("../views/request/category/ca_team.vue");
const Request_Ca_Form = () => import("../views/request/category/ca_form.vue");
const Request_Config_Auth_Sign = () => import("../views/request/config/set_auth_sign.vue");
const Request_Config_Number = () => import("../views/request/config/set_number_request.vue");

const router = createRouter({
    history: createWebHistory(
        import.meta.env.BASE_URL),
    routes: [{
            path: "/",
            name: "homeview",
            component: HomeView,
        },
        // {
        //     path: "/scraper",
        //     name: "scraper",
        //     component: ScraperView,
        // },
        {
            path: "/table",
            name: "table",
            component: SQLTableView,
        },
        {
            path: "/organization",
            name: "organization",
            component: Organization,
        },
        //Canva
        {
            path: "/canva",
            name: "canva",
            component: CanvaView,
        },
        //Hệ thống
        {
            path: "/module",
            name: "module",
            component: ModulesView,
        },
        {
            path: "/user",
            name: "user",
            component: UserView,
        },
        {
            path: "/history",
            name: "history",
            component: WebAcessView,
        },
        {
            path: "/logs",
            name: "logs",
            component: LogsView,
        },
        {
            path: "/testcase",
            name: "testcase",
            component: TestCaseView,
        },
        {
            path: "/config",
            name: "config",
            component: ConfigView,
        },
        {
            path: "/sql",
            name: "sql",
            component: SQLView,
        },
        {
            path: "/role",
            name: "role",
            component: RolesView,
        },
        {
            path: "/login",
            name: "login",
            component: LoginView,
        },
        {
            path: "/donvi",
            name: "donvi",
            component: DonviView,
        },
        {
            path: "/group-type",
            name: "group-type",
            component: GroupTypeView,
        },
        {
            path: "/group",
            name: "group",
            component: GroupView,
        },
        {
            path: "/config-organizational",
            name: "config-organizational",
            component: ConfigDonviView,
        },
        {
            path: "/upload",
            name: "upload",
            component: UploadView,
        },
        {
            path: "/options",
            name: "options",
            component: OptionsAccount,
        },
        {
            path: "/data-using",
            name: "data-using",
            component: DataUsing,
        },
        {
            path: "/error-404",
            name: "error-404",
            component: Error,
        },

        //Danh mục
        {
            path: "/folder",
            name: "folder",
            component: FolderView,
        },

        //Sys
        //CMS
        {
            path: "/cms",
            name: "cms",
            component: CMSView,
            children: [
                { path: "", component: CMSHomeView },
                {
                    path: "log",
                    name: "log",
                    component: LangView,
                },
                {
                    path: "lang",
                    name: "lang",
                    component: LangView,
                },
                {
                    path: "topic-flag",
                    name: "topic-flag",
                    component: TopicFlagView,
                },
                {
                    path: "topic",
                    name: "topic",
                    component: TopicView,
                },
                {
                    path: "new",
                    name: "new",
                    component: NewsView,
                },
                {
                    path: "slideshow",
                    name: "slideshow",
                    component: SlideShowView,
                },
            ],
        },
        //API
        {
            path: "/project",
            name: "project",
            component: Project,
        },
        {
            path: "/api",
            name: "api",
            component: Api,
        },
        {
            path: "/table",
            name: "table",
            component: Table,
        },
        {
            path: "/plugin",
            name: "plugin",
            component: Plugin,
        }, //Từ điển
        {
            path: "/browse",
            name: "browse",
            component: BrowseRole,
        },
        {
            path: "/docRole",
            name: "docRole",
            component: DocRole,
        },
        {
            path: "/dispatch",
            name: "dispatch",
            component: Dispatch,
        },
        {
            path: "/email",
            name: "email",
            component: Email,
        },
        {
            path: "/cagroup",
            name: "cagroup",
            component: Cagroup,
        },
        {
            path: "/field",
            name: "field",
            component: Field,
        },
        {
            path: "/issueplace",
            name: "issueplace",
            component: IssuePlace,
        },
        {
            path: "/receveplace",
            name: "receveplace",
            component: RecevePlace,
        },
        {
            path: "/security",
            name: "security",
            component: Security,
        },
        {
            path: "/sendway",
            name: "sendway",
            component: SendWay,
        },
        {
            path: "/signer",
            name: "signer",
            component: Signer,
        },
        {
            path: "/urgency",
            name: "urgency",
            component: Urgency,
        },
        {
            path: "/position",
            name: "position",
            component: Positions,
        },
        {
            path: "/place",
            name: "place",
            component: Places,
        },
        {
            path: "/caposition",
            name: "caposition",
            component: CaPosition,
        },
        {
            path: "/tem",
            name: "tem",
            component: Tem,
        },
        {
            path: "/emote",
            name: "emote",
            component: Emote,
        },
        {
            path: "/gif",
            name: "Gif",
            component: Gif,
        },
        {
            path: "/type",
            name: "type",
            component: Type,
        },
        {
            path: "/tags",
            name: "tags",
            component: Tags,
        },
        {
            path: "/status",
            name: "status",
            component: Status,
        },
        {
            path: "/project",
            name: "project",
            component: Project,
        },
        {
            path: "/api",
            name: "api",
            component: Api,
        },
        {
            path: "/table",
            name: "table",
            component: Table,
        },
        {
            path: "/plugin",
            name: "plugin",
            component: Plugin,
        },
        //Task
        {
            path: "/project/task",
            name: "task/project",
            component: Task,
        },

        {
            path: "/taskgroup",
            name: "taskgroup",
            component: TaskGroup,
        },
        {
            path: "/taskcheck",
            name: "taskcheck",
            component: TaskCheck,
        },
        {
            path: "/taskreport/mainreport",
            name: "mainreport",
            component: MainReport,
        },
        {
            path: "/taskreport/checklistreport",
            name: "checklistreport",
            component: CheckListReport,
        },
        //Tin tức

        {
            path: "/news/newsmain",
            name: "news/newsmain",
            component: NewsMain,
        },
        {
            path: "/news/direct",
            name: "/news/direct",
            component: DirectOperator,
        },
        {
            path: "/news/direct/:name",
            name: "/news/direct/details",
            component: NewDetails,
        },
        {
            path: "/news/direct/keywords/:name",
            name: "newskeywords",
            component: NewsKeyWords,
        },

        //BirthDay
        {
            path: "/birthday",
            name: "birthday",
            component: BirthDay,
        },
        {
            path: "/birthday/send",
            name: "hppd",
            component: SendCongs,
        },
        //Video

        {
            path: "/news/videosmain",
            name: "/news/videosmain",
            component: VideosMain,
        },
        {
            path: "/news/videosview/:name",
            name: "news/videosview/detail",
            component: VideoDetails,
        },
        {
            path: "/news/videossearch/:name",
            name: "news/videosview/search",
            component: VideoSearch,
        },
        {
            path: "/news/videosview",
            name: "/news/videosview",
            component: VideosView,
        },
        //Trình diễn

        {
            path: "/news/showsmain",
            name: "showsmain",
            component: ShowsMain,
        },

        {
            path: "/news/shows",
            name: "/news/shows",
            component: Shows,
        },
        {
            path: "/news/shows/:name",
            name: "/news/shows/details",
            component: ShowDetails,
        },

        //Start Calendar
        {
            path: "/calendar/main",
            name: "calendarmain",
            component: calendarMain,
        },
        {
            path: "/calendar/detail/:id",
            name: "calendardetail",
            component: calendarDetail,
        },
        {
            path: "/calendar/reserve",
            name: "calendarreserve",
            component: calendarReserve,
        },
        {
            path: "/calendar/pendding",
            name: "calendarpendding",
            component: calendarPendding,
        },
        {
            path: "/calendar/follow",
            name: "calendarfollow",
            component: calendarFollow,
        },
        {
            path: "/calendar/enact",
            name: "calendarenact",
            component: calendarEnact,
        },
        {
            path: "/calendar/plantripdetail/:id",
            name: "calendarplantripdetail",
            component: calendarPlantripDetail,
        },
        {
            path: "/calendar/plantripreserve",
            name: "calendarplantripreserve",
            component: calendarPlantripReserve,
        },
        {
            path: "/calendar/plantrippendding",
            name: "calendarplantrippendding",
            component: calendarPlantripPendding,
        },
        {
            path: "/calendar/plantripfollow",
            name: "calendarplantripfollow",
            component: calendarPlantripFollow,
        },
        {
            path: "/calendar/plantripenact",
            name: "calendarplantripenact",
            component: calendarPlantripEnact,
        },
        {
            path: "/calendar/dutyreserve",
            name: "calendardutyreserve",
            component: calendarDutyReserve,
        },
        {
            path: "/calendar/dutypendding",
            name: "calendardutypendding",
            component: calendarDutyPendding,
        },
        {
            path: "/calendar/dutyfollow",
            name: "calendardutyfollow",
            component: calendarDutyFollow,
        },
        {
            path: "/calendar/dutyapproved",
            name: "calendardutyapproved",
            component: calendarDutyApproved,
        },
        {
            path: "/calendar/boardroom",
            name: "calendarboardroom",
            component: calendarBoardroom,
        },
        {
            path: "/calendar/car",
            name: "calendarcar",
            component: calendarCar,
        },
        {
            path: "/calendar/position",
            name: "calendarposition",
            component: calendarPosition,
        },
        {
            path: "/calendar/config",
            name: "calendarconfig",
            component: calendarConfig,
        },
        {
            path: "/calendar/dutyconfig",
            name: "calendardutyconfig",
            component: calendarDutyConfig,
        },
        //End Calendar
        //Statistical
        {
            path: "/statistical/statistical_main",
            name: "statisticalmain",
            component: statisticalMain,
        },
        {
            path: "/statistical/config",
            name: "statisticalconfig",
            component: statisticalConfig,
        },
        {
            path: "/statistical/chart/:id",
            name: "statisticalchart",
            component: statisticalChart,
        },
        //End statisticalMain
        // Doc
        {
            path: "/doc/receive/:id",
            name: "docreceive/detail",
            component: DocReceive_Detail,
        },
        {
            path: "/doc/receive",
            name: "docreceive",
            component: DocReceive,
        },
        {
            path: "/doc/send",
            name: "docsend",
            component: DocSend,
        },
        {
            path: "/doc/store",
            name: "docstore",
            component: DocStore,
        },
        {
            path: "/doc/report/docreceive",
            name: "reportdocreceive",
            component: DocReceiveReport,
        },
        {
            path: "/doc/report/docsend",
            name: "reportdocsend",
            component: DocSendReport,
        },

        {
            path: "/doc/report/docinternal",
            name: "reportdocinternal",
            component: DocInternalReport,
        },

        {
            path: "/doc/report/docsendemail",
            name: "reportsendemail",
            component: DocSendEmailReport,
        },
        {
            path: "/doc/config/docnumber",
            name: "configdocnumber",
            component: DocConfigNumber,
        },
        {
            path: "/doc/config/axis",
            name: "DocAxisConfig",
            component: DocAxisConfig,
        },
        {
            path: "/doc/config/reservation",
            name: "DocReservationNumber",
            component: DocReservationNumber,
        },
        // Luật Việt Name
        {
            path: "/law/lawmain",
            name: "lawmain",
            component: LawMain,
        },

        // Đặt cơm
        {
            path: "/booking/bookingmeal",
            name: "bookingmeal",
            component: BookingMeal,
        },
        {
            path: "/booking/bookingmeal/:id",
            name: "bookingmeal/detail",
            component: BookingMealDetail,
        },
        {
            path: "/booking/userbooking",
            name: "userbooking",
            component: UserBooking,
        },
        {
            path: "/booking/weeksreport",
            name: "weeksreport",
            component: WeeksReport,
        },
        {
            path: "/booking/usersreport",
            name: "usersreport",
            component: UsersReport,
        },
        {
            path: "/booking/config",
            name: "configbooking",
            component: BookingConfig,
        },
        {
            path: "/booking/booking_daily",
            name: "bookingdaily",
            component: BookingDaily,
        },
        // Danh mục Luật
        // Law - Ngôn ngữ
        {
            path: "/law/lawdoclanguage",
            name: "lawdoclanguage",
            component: LawDocLanguage,
        },
        // Law - Loại văn bản
        {
            path: "/law/lawdoctypes",
            name: "lawdoctypes",
            component: LawDocTypes,
        },
        // Law - Cơ quan ban hành
        {
            path: "/law/lawdocissueplaces",
            name: "lawdocissueplaces",
            component: LawDocIssuePlaces,
        },
        // Law - Lĩnh vực hoạt động
        {
            path: "/law/lawdocfields",
            name: "lawdocfields",
            component: LawDocFields,
        },
        // Law - Người ký
        {
            path: "/law/lawdocsigners",
            name: "lawdocsigners",
            component: LawDocSigners,
        },
        //-----Tài sản-----

        // Thẻ tài sản
        {
            path: "/device/category/card",
            name: "card",
            component: DeviceCard,
        },
        // Từ điển tài sản
        {
            path: "/device/category/groups",
            name: "devicegroups",
            component: DeviceGroups,
        },
        // Loại tài sản
        {
            path: "/device/category/type",
            name: "typedevice",
            component: DeviceType,
        },
        // Đơn vị tính
        {
            path: "/device/category/unit",
            name: "deviceunit",
            component: DeviceUnit,
        },
        // Nhà cung cấp
        {
            path: "/device/category/provider",
            name: "deviceprovider",
            component: DeviceProvider,
        },
        // Kho
        {
            path: "/device/category/warehouse",
            name: "devicewarehouse",
            component: DeviceWareHouse,
        },
        // Tài sản
        {
            path: "/device/category/devicemain",
            name: "devicedevicemain",
            component: DeviceMain,
        },
        // cấp phát
        {
            path: "/device/accepthandover",
            name: "accepthandover",
            component: AcceptHandover,
        },
        // Phiếu chờ duyệt
        {
            path: "/device/doc_approved",
            name: "doc_approved",
            component: DeviceApproved,
        },
        {
            path: "/device/follows",
            name: "follows",
            component: DeviceFollows,
        },

        {
            path: "/device/report/device_inventory",
            name: "device_inventoryReport",
            component: DeviceInventoryReport,
        },
        {
            path: "/device/report/device_expiration",
            name: "device_expiration",
            component: DeviceExpiration,
        },
        {
            path: "/device/report/device_insurance",
            name: "device_insurance",
            component: DeviceInsurance,
        },
        {
            path: "/device/configdehandover",
            name: "configdehandover",
            component: ConfigDeHandover,
        },
        {
            path: "/device/my_assets",
            name: "myassets",
            component: MyAssets,
        },


        //Cấu hình
        {
            path: "/device/config_groups",
            name: "deviceconfig_groups",
            component: ConfigGroups,
        },
        {
            path: "/device/config_number",
            name: "deviceconfig_number",
            component: ConfigNumber,
        },
        {
            path: "/device/repair",
            name: "devicerepair",
            component: DeviceRepair,
        },
        {
            path: "/device/inventory",
            name: "deviceinventory",
            component: DeviceInventory,
        },
        {
            path: "/device/acceptinventory",
            name: "deviceacceptinventory",
            component: DeviceAcceptInventory,
        },
        {
            path: "/device/recall",
            name: "devicerecall",
            component: DeviceRecall,
        },
        {
            path: "/device/log",
            name: "devicedevice_log",
            component: DeviceLog,
        },
        {
            path: "/device/handover",
            name: "devicehandover",
            component: Handover,
        },
        {
            path: "/device/category/manufacturer",
            name: "devicemanufacturer",
            component: DeviceManufacturer,
        },
        {
            path: "/device/category/status",
            name: "devicestatus",
            component: DeviceStatus,
        }, {
            path: "/device/my_assets",
            name: "myAssets",
            component: MyAssets,
        },

        // Chat
        // Tin nhắn
        {
            path: "/chat/chat_message",
            name: "chat_message",
            component: Chat_Message,
        },
        // Danh bạ
        {
            path: "/chat/chat_contact",
            name: "chat_contact",
            component: Chat_Contact,
        },
        {
            path: "/chat/chat_message/:id",
            name: "chat_message/detail",
            component: Chat_Detail,
        },

        {
            path: "/files/file_folder",
            name: "file_folder",
            component: FileFolder,
        },
        {
            path: "/files/file_main",
            name: "file_main",
            component: FileMain,
        },
        {
            path: "/files/file_log",
            name: "file_log",
            component: FileLog,
        },
        {
            path: "/files/file_log_views",
            name: "file_log_views",
            component: FileLogViews,
        },
        {
            path: "/files/file_log_users",
            name: "file_log_users",
            component: FileLogUsers,
        },
        {
            path: "/files/file_config",
            name: "file_config",
            component: FileConfig,
        },
        // Nhóm công việc CA
        {
            path: "/tasks/category/taskgroups",
            name: "taskgroup",
            component: TaskGroups,
        },
        {
            path: "/tasks/projectmain/projectgroups",
            name: "projectgroup",
            component: ProjectGroup,
        },
        {
            path: "/tasks/project",
            name: "projectmain",
            component: ProjectMain,
        },
        {
            path: "/tasks/taskmain",
            name: "taskmain",
            component: TaskOrigin,
        },
        {
            path: "/tasks/taskmain/:id",
            name: "taskmaindetail",
            component: TaskOriginDetail,
        },
        {
            path: "/tasks/config/weight",
            name: "taskWeights",
            component: taskWeights,
        },
        {
            path: "/department/configuration",
            name: "taskdepartmentconfiguration",
            component: TaskDepartmentConfiguration,
        },
        {
            path: "/tasks/report/task_personal",
            name: "taskreportpersonal",
            component: TaskReportPersonal,
        },
        {
            path: "/tasks/report/task_department",
            name: "taskreportdepartment",
            component: TaskReportDepartment,
        },
        {
            path: "/tasks/config/group",
            name: "BrowseGroup",
            component: BrowseGroup,
        },

        // {
        //     path: "/tasks/review_person_report",
        //     name: "TaskReviewReport",
        //     component: TaskReviewReport,
        // },
        {
            path: "/doc/config/browserbydept",
            name: "BrowserByDepartment",
            component: BrowserByDepartment,
        },
        //Báo cáo
        {
            path: "/reportmodule/history",
            name: "reportmodulehistory",
            component: WebAcessView,
        },
        {
            path: "/reportmodule/docreceive",
            name: "reportmoduledocreceive",
            component: DocReceiveReport,
        },
        {
            path: "/reportmodule/config",
            name: "reportmodulecofig",
            component: ReportConfig,
        },
        {
            path: "/reportmodule/docsend",
            name: "reportmoduledocsend",
            component: DocSendReport,
        },
        {
            path: "/reportmodule/docinternal",
            name: "reportmoduledocinternal",
            component: DocInternalReport,
        },
        {
            path: "/reportmodule/device_expiration",
            name: "reportdevice_expiration",
            component: DeviceExpiration,
        },
        {
            path: "/reportmodule/device_inventory",
            name: "reportdevice_inventory",
            component: DeviceInventory,
        },
        {
            path: "/reportmodule/task_department",
            name: "reportmoduletaskdepartment",
            component: TaskReportDepartment,
        },
        {
            path: "/reportmodule/task_personal",
            name: "reportmoduletaskpersonal",
            component: TaskReportPersonal,
        },
        // {
        //     path: "/tasks/config/person",
        //     name: "TaskPersonConfig",
        //     component: TaskPersonConfig,
        // },
        {
            path: "/config_screentv",
            name: "ConfigScreenTV",
            component: ConfigScreenTV,
        },
        {
            path: "/screen_tivi",
            name: "ScreenTV",
            component: ScreenTV,
        },
        {
            path: "/tasks/person_report",
            name: "TaskPersonCreateReport",
            component: TaskPersonCreateReport,
        },
        {
            path: "/tasks/review_person_report",
            name: "TaskReviewReport",
            component: TaskReviewReport,
        },
        {
            path: "/tasks/config/person",
            name: "TaskPersonConfig",
            component: TaskPersonConfig,
        },
        {
            path: "/task/total_reports",
            name: "totalReport",
            component: totalReport,
        },
        //Danh mục của Doc
        {
            path: "/doc/cagroup",
            name: "Cagroup",
            component: Cagroup,
        },
        {
            path: "/doc/position",
            name: "CaPosition",
            component: CaPosition,
        },
        {
            path: "/doc/issueplace",
            name: "IssuePlace",
            component: IssuePlace,
        },
        {
            path: "/doc/issueplace",
            name: "IssuePlace",
            component: IssuePlace,
        },
        {
            path: "/doc/urgency",
            name: "Urgency",
            component: Urgency,
        },
        {
            path: "/doc/security",
            name: "Security",
            component: Security,
        },
        {
            path: "/doc/type",
            name: "Type",
            component: Type,
        },
        {
            path: "/doc/type",
            name: "Type",
            component: Type,
        },
        {
            path: "/doc/field",
            name: "Field",
            component: Field,
        },
        {
            path: "/doc/signer",
            name: "Signer",
            component: Signer,
        },
        {
            path: "/doc/sendway",
            name: "SendWay",
            component: SendWay,
        },
        {
            path: "/doc/receveplace",
            name: "RecevePlace",
            component: RecevePlace,
        },
        {
            path: "/doc/receveplace",
            name: "RecevePlace",
            component: RecevePlace,
        },
        {
            path: "/doc/dispatch",
            name: "Dispatch",
            component: Dispatch,
        },
        {
            path: "/doc/email",
            name: "Email",
            component: Email,
        },
        {
            path: "/doc/docRole",
            name: "DocRole",
            component: DocRole,
        },
        {
            path: "/tasks/dashboard",
            name: "TaskDashboard",
            component: TaskDashboard,
        },
        // hrm 
        // {
        //     path: "/hrm/category/interview",
        //     name: "Interview",
        //     component: Interview,
        // },
        {
            path: "/hrm/hrm_profile",
            name: "Hrm_Info",
            component: Hrm_Info,
        },
        {
            path: "/sqldb",
            name: "SQLDB_Query",
            component: SQLDB_Query,
        },
        {
            path: "/hrm/profile",
            name: "profile",
            component: HrmProfile,
        },
        {
            path: "/hrm/profile/:id",
            name: "profileinfo",
            component: HrmProfileInfo,
        },
        {
            path: "/hrm/contract",
            name: "contract",
            component: HrmContract,
        },
        // CA_HRM
        {
            path: "/hrm/category/academic_level",
            name: "academiclevel",
            component: AcademicLevel,
        },
        {
            path: "/hrm/category/ca_bank",
            name: "caHRMBank",
            component: caHRMBank,
        },
        {
            path: "/hrm/category/ca_certificate",
            name: "caHRMCertificate",
            component: caHRMCertificate,
        },
        {
            path: "/hrm/category/ca_classification",
            name: "caHRMClassification",
            component: caHRMClassification,
        },
        {
            path: "/hrm/category/ca_culturalLevel",
            name: "caHRMCulturalLevel",
            component: caHRMCulturalLevel,
        },
        {
            path: "/hrm/category/ca_degree",
            name: "caHRMDegree",
            component: caHRMDegree,
        },
        {
            path: "/hrm/category/ca_ethnic",
            name: "caHRMEthnic",
            component: caHRMEthnic,
        },
        {
            path: "/hrm/category/ca_evaluationCriteria",
            name: "caHRMEvaluationCriteria",
            component: caHRMEvaluationCriteria,
        },
        {
            path: "/hrm/category/ca_faculty",
            name: "caHRMFaculty",
            component: caHRMFaculty,
        },
        {
            path: "/hrm/category/ca_formality",
            name: "caHRMFormality",
            component: caHRMFormality,
        },
        {
            path: "/hrm/category/ca_formTraining",
            name: "caHRMFormTraining",
            component: caHRMFormTraining,
        },
        {
            path: "/hrm/category/ca_identityPapers",
            name: "caHRMIdentityPapers",
            component: caHRMIdentityPapers,
        },
        {
            path: "/hrm/category/ca_identityPlace",
            name: "caHRMIdentityPlace",
            component: caHRMIdentityPlace,
        },
        {
            path: "/hrm/category/ca_informaticLevel",
            name: "caHRMInformaticLevel",
            component: caHRMInformaticLevel,
        },
        {
            path: "/hrm/category/ca_interviewRound",
            name: "caHRMInterviewRound",
            component: caHRMInterviewRound,
        },
        {
            path: "/hrm/category/ca_languageLevel",
            name: "caHRMLanguageLevel",
            component: caHRMLanguageLevel,
        },
        {
            path: "/hrm/category/ca_learningPlace",
            name: "caHRMLearningPlace",
            component: caHRMLearningPlace,
        },
        {
            path: "/hrm/category/ca_managementMajor",
            name: "caHRMManagementMajor",
            component: caHRMManagementMajor,
        },
        {
            path: "/hrm/category/ca_managementState",
            name: "caHRMManagementState",
            component: caHRMManagementState,
        },
        {
            path: "/hrm/category/ca_nationality",
            name: "caHRMNationality",
            component: caHRMNationality,
        },
        {
            path: "/hrm/category/ca_personnelLevel",
            name: "caHRMPersonnelLevel",
            component: caHRMPersonnelLevel,
        },
        {
            path: "/hrm/category/ca_politicalTheory",
            name: "caHRMPoliticalTheory",
            component: caHRMPoliticalTheory,
        },
        {
            path: "/hrm/category/ca_professionalWork",
            name: "caHRMProfessionalWork",
            component: caHRMProfessionalWork,
        },
        {
            path: "/hrm/category/ca_relationship",
            name: "caHRMRelationship",
            component: caHRMRelationship,
        },
        {
            path: "/hrm/category/ca_religion",
            name: "caHRMReligion",
            component: caHRMReligion,
        },
        {
            path: "/hrm/category/ca_specialization",
            name: "caHRMSpecialization",
            component: caHRMSpecialization,
        },
        {
            path: "/hrm/category/ca_title",
            name: "caHRMTitle",
            component: caHRMTitle,
        },
        {
            path: "/hrm/category/ca_typeContract",
            name: "caHRMTypeContract",
            component: caHRMTypeContract,
        },
        {
            path: "/hrm/category/ca_vacancy",
            name: "caHRMVacancy",
            component: caHRMVacancy,
        },
        {
            path: "/hrm/category/ca_wage",
            name: "caHRMWage",
            component: caHRMWage,
        },

        {
            path: "/hrm/category/ca_leavingreason",
            name: "caHRMLeavingReason",
            component: caLeavingReason,
        },
        {
            path: "/hrm/category/ca_experience",
            name: "caHRMExperience",
            component: caExperience,
        },
        {
            path: "/hrm/category/ca_candidate",
            name: "caHRMCandidate",
            component: caCandidate,
        },
        {
            path: "/hrm/category/ca_classroom",
            name: "caHRMClassroom",
            component: caClassroom,
        },
        {
            path: "/hrm/category/ca_receipt",
            name: "caHRMReceipt",
            component: caReceipt,
        },
        {
            path: "/hrm/category/ca_enectinggroup",
            name: "caHRMEnectingGroup",
            component: caEnectingGroup,
        },

        //HRM
        {
            path: "/hrm/hrm_training",
            name: "HRM_Training",
            component: HRM_Training,
        }, // Tài liệu
        {
            path: "/files/file_main/:id/:type",
            name: "files/file_main_detail",
            component: FileMain_Detail,
        },
        {
            path: "/hrm/insurance",
            name: "insurance",
            component: Insurance,
        },
        {
            path: "/hrm/hrm_file",
            name: "Hrm_File",
            component: Hrm_File,
        },
        {
            path: "/hrm/recruitment/campaign",
            name: "Hrm_campaign",
            component: Hrm_campaign,
        }, 
        {
            path: "/hrm/hrm_paycheck",
            name: "Hrm_paycheck",
            component: Hrm_paycheck,
        },
        // Request
        {
          path: "/request/dashboard",
          name: "Request_Dashboard",
          component: Request_Dashboard,
        },
        {
          path: "/request/request",
          name: "Request_Request",
          component: Request_Request,
        },
        {
          path: "/request/team",
          name: "Request_Team",
          component: Request_Team,
        },
        {
          path: "/request/document",
          name: "Request_Document",
          component: Request_Document,
        },
        // Request/Category
        {
          path: "/request/category/ca_group",
          name: "Request_Ca_Group",
          component: Request_Ca_Group,
        },
        {
          path: "/request/category/ca_groupteam",
          name: "Request_Ca_GroupTeam",
          component: Request_Ca_GroupTeam,
        },
        {
          path: "/request/category/ca_team",
          name: "Request_Ca_Team",
          component: Request_Ca_Team,
        },
        {
          path: "/request/category/ca_form",
          name: "Request_Ca_Form",
          component: Request_Ca_Form,
        },
        // Request/Config
        {
          path: "/request/config/ca_team",
          name: "Request_Config_Auth_Sign",
          component: Request_Config_Auth_Sign,
        },
        {
          path: "/request/config/ca_form",
          name: "Request_Config_Number",
          component: Request_Config_Number,
        },
    ],
});
// router.beforeEach((to, from) => {
//   return true
// })
export default router;