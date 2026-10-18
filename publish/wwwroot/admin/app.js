// ==========================================================================
// STATE MANAGEMENT & CONSTANTS
// ==========================================================================
const API_URL = window.location.origin;

let state = {
    token: localStorage.getItem('witt_admin_token') || sessionStorage.getItem('witt_admin_token') || '',
    user: null,
    activeTab: 'dashboard',
    stops: [],  // Cache of stops for route builder and dropdowns
    trains: [], // Cache of trains
    suggestions: [], // Cache of suggestions for view details
    language: localStorage.getItem('witt_admin_language') || 'en',
    statusTags: [],
    crowdLevels: [],
    notifications: [],
    railwayPaths: []
};

// Details View States for pagination and filtering
let detailsState = {
    trainTrips: [],
    trainTripsPage: 1,
    trainTripsDateFrom: '',
    trainTripsDateTo: '',
    trainFollowers: [],
    trainFollowersPage: 1,
    trainFollowersSearch: '',

    tripTrips: [],
    tripTripsPage: 1,
    tripTripsDateFrom: '',
    tripTripsDateTo: '',
    tripFollowers: [],
    tripFollowersPage: 1,
    tripFollowersSearch: ''
};

// Safe JSON parsing for user session to prevent start-up exceptions
try {
    const userStr = localStorage.getItem('witt_admin_user') || sessionStorage.getItem('witt_admin_user');
    state.user = userStr ? JSON.parse(userStr) : null;
} catch (e) {
    console.error('Failed to parse user session from storage:', e);
    // Clean corrupt data to prevent loop
    localStorage.removeItem('witt_admin_token');
    localStorage.removeItem('witt_admin_user');
    sessionStorage.removeItem('witt_admin_token');
    sessionStorage.removeItem('witt_admin_user');
    state.token = '';
    state.user = null;
}

// Map TripStatus enum to text and CSS classes
const TRIP_STATUS_MAP = {
    0: { text: 'Scheduled', class: 'scheduled' },
    1: { text: 'Departed', class: 'departed' },
    2: { text: 'In Transit', class: 'intransit' },
    3: { text: 'Arrived', class: 'arrived' },
    4: { text: 'Cancelled', class: 'cancelled' },
    5: { text: 'Delayed', class: 'delayed' },
    'Scheduled': { text: 'Scheduled', class: 'scheduled' },
    'Departed': { text: 'Departed', class: 'departed' },
    'InTransit': { text: 'In Transit', class: 'intransit' },
    'Arrived': { text: 'Arrived', class: 'arrived' },
    'Cancelled': { text: 'Cancelled', class: 'cancelled' },
    'Delayed': { text: 'Delayed', class: 'delayed' }
};

// ==========================================================================
// TRANSLATION DICTIONARIES & HELPER ENGINE
// ==========================================================================
const TRANSLATIONS = {
    en: {
        // App Portal Titles
        dashboard: "Dashboard",
        trains: "Trains",
        stops: "Stops / Stations",
        trips: "Trips",
        suggestions: "Suggestions",
        lostfound: "Lost & Found",
        railway_paths: "Railway Paths",
        tab_railway_paths_title: "Manage Railway Paths",
        tab_railway_paths_subtitle: "Define physical railway tracks between stations using GeoJSON.",
        live_updates: "Live Updates",
        tab_updates_title: "Moderate Live Updates",
        tab_updates_subtitle: "Approve user-submitted updates and review removal requests.",
        pendingApproval: "Pending Approval",
        noPendingUpdates: "No pending live updates.",
        removalRequests: "Removal Requests",
        noRemovalRequests: "No active removal requests.",
        reject: "Reject",
        approve: "Approve",
        denyRemoval: "Deny Removal",
        confirmRemoval: "Confirm Removal",
        deny_removal_success: "Live update removal request denied.",
        
        tab_dashboard_title: "Dashboard",
        tab_dashboard_subtitle: "Overview of platform activities and statistics.",
        tab_trains_title: "Manage Trains",
        tab_trains_subtitle: "Configure trains, stops, routes, and schedule timing.",
        tab_stops_title: "Manage Stops",
        tab_stops_subtitle: "Add, modify or delete railway stations and GPS coordinates.",
        tab_trips_title: "Train Trips",
        tab_trips_subtitle: "Monitor and schedule daily train trips, delays, and statuses.",
        tab_suggestions_title: "User Suggestions",
        tab_suggestions_subtitle: "Review suggested train routes submitted by commuters.",
        tab_lostfound_title: "Moderate Lost & Found",
        tab_lostfound_subtitle: "Review commuter reports, manage status transitions, and moderate comments.",
        tab_train_details_title: "Train Details",
        tab_train_details_subtitle: "View specifications, routes, and followers of this train.",
        tab_trip_details_title: "Trip Details",
        tab_trip_details_subtitle: "View trip schedule, updates, and followers of this trip.",
        back_to_list: "Back to List",

        // Stats Cards
        stat_trains_lbl: "Trains",
        stat_stops_lbl: "Stops",
        stat_trips_lbl: "Active Trips",
        stat_suggestions_lbl: "Pending Suggestions",
        
        // Buttons
        add_train: "Add Train",
        add_stop: "Add Stop / Station",
        schedule_trip: "Schedule Trip",
        logout: "Sign Out",
        admin_portal: "Admin Portal",
        
        // Search & Filters
        search_train_placeholder: "Search by number or name...",
        search_stop_placeholder: "Search by code or name...",
        search_lostfound_placeholder: "Search title or train...",
        filter_all_types: "All Types",
        filter_all_statuses: "All Statuses",
        type_lost: "Lost",
        type_found: "Found",
        
        // Table Headers
        table_col_code: "Code",
        table_col_station_name: "Station Name",
        table_col_city: "City",
        table_col_lat: "Latitude",
        table_col_lng: "Longitude",
        table_col_description: "Description",
        table_col_actions: "Actions",
        table_col_train_no: "Train No.",
        table_col_train_name: "Train Name",
        table_col_stops_seq: "Stops Sequence",
        table_col_status: "Status",
        table_col_train: "Train",
        table_col_trip_date: "Trip Date",
        table_col_actual_dep: "Actual Departure",
        table_col_actual_arr: "Actual Arrival",
        table_col_followers: "Followers",
        table_col_name: "Name",
        table_col_proposed_route: "Proposed Route",
        table_col_suggested_by: "Suggested By",
        table_col_date: "Date",
        table_col_type: "Type",
        table_col_title: "Title",
        table_col_author: "Author",
        table_col_comments: "Comments",
        
        // Status maps
        status_scheduled: "Scheduled",
        status_departed: "Departed",
        status_intransit: "In Transit",
        status_arrived: "Arrived",
        status_cancelled: "Cancelled",
        status_delayed: "Delayed",
        status_active: "Active",
        status_inactive: "Inactive",

        // Crowd tags
        status_ontime: "On Time",
        status_crowded: "Crowded",
        status_empty: "Empty",
        status_atstation: "At Station",
        status_new: "New",
        status_published: "Published",
        status_rejected: "Rejected",
        status_closed: "Closed",
        
        // Messages
        loading_updates: "Loading recent updates...",
        no_updates: "No updates posted today.",
        api_connected: "API Connected",
        loading_trains: "Loading trains...",
        no_trains: "No trains found. Click 'Add Train' to create one.",
        view_sequence: "View Sequence",
        loading_stops: "Loading stops...",
        no_stops: "No stops found. Click 'Add Stop' to create one.",
        loading_trips: "Loading scheduled trips...",
        no_trips: "No trips scheduled. Click 'Schedule Trip' to create one.",
        loading_suggestions: "Loading pending suggestions...",
        no_suggestions: "No pending train suggestions for review.",
        loading_lostfound: "Loading lost & found posts...",
        no_lostfound: "No matching lost & found posts found.",
        no_comments: "No comments on this post.",
        
        // Profile
        profile_settings_title: "Profile Settings",
        profile_display_name_lbl: "Display Name",
        profile_bio_lbl: "Bio",
        profile_avatar_file_lbl: "Profile Picture",
        save_profile_btn: "Save Profile",
        tab_profile_title: "Admin Profile",
        tab_profile_subtitle: "View and edit your administrator account settings.",
        
        // Modals
        modal_stop_add_title: "Add Stop",
        modal_stop_edit_title: "Edit Stop",
        modal_stop_code: "Stop Code (Unique)",
        modal_stop_name_ar: "Name (AR)",
        modal_stop_name_en: "Name (EN)",
        modal_stop_city_ar: "City (AR)",
        modal_stop_city_en: "City (EN)",
        modal_stop_desc_ar: "Description (AR)",
        modal_stop_desc_en: "Description (EN)",
        modal_stop_lat: "Latitude",
        modal_stop_lng: "Longitude",
        modal_stop_map_lbl: "Location Map (Drag marker or click map to set coordinates)",
        modal_cancel: "Cancel",
        modal_save_stop: "Save Stop",
        
        modal_train_add_title: "Add Train Route",
        modal_train_edit_title: "Edit Train Route",
        modal_train_number: "Train Number (Unique)",
        modal_train_name_en: "Train Name (EN)",
        modal_train_name_ar: "Train Name (AR)",
        modal_train_desc_en: "Description (EN)",
        modal_train_desc_ar: "Description (AR)",
        modal_train_route_stops: "Route Stops Schedule",
        modal_train_add_stop: "Add Stop to Route",
        modal_train_save: "Save Train",
        modal_train_seq: "Seq",
        modal_train_station: "Station / Stop",
        modal_train_arr: "Arr Time",
        modal_train_dep: "Dep Time",
        
        modal_trip_title: "Schedule New Trip",
        modal_trip_train: "Select Train",
        modal_trip_date: "Trip Date",
        modal_trip_status: "Status",
        modal_trip_submit: "Schedule",
        
        modal_trip_update_title: "Update Trip Status",
        modal_trip_actual_dep: "Actual Departure Time (Optional)",
        modal_trip_actual_arr: "Actual Arrival Time (Optional)",
        modal_trip_update_btn: "Update Status",
        
        modal_suggestion_title: "Review Train Suggestion",
        modal_suggestion_train: "Train",
        modal_suggestion_route: "Proposed Route",
        modal_suggestion_author: "Suggested By",
        modal_suggestion_action: "Action",
        modal_suggestion_approve: "Approve",
        modal_suggestion_reject: "Reject",
        modal_suggestion_notes: "Notes / Feedback to User",
        modal_suggestion_submit: "Submit Review",
        
        modal_lostfound_title: "Moderate Lost & Found Post",
        modal_lostfound_details: "Post Details",
        modal_lostfound_post_title: "Title",
        modal_lostfound_post_desc: "Description",
        modal_lostfound_post_type: "Type",
        modal_lostfound_post_train: "Train Number",
        modal_lostfound_post_contact: "Contact Info",
        modal_lostfound_post_status: "Moderation Status",
        modal_lostfound_save_btn: "Save Changes",
        modal_lostfound_comments: "Comments Moderation",
        modal_lostfound_admin_comment: "Add Admin Comment",
        modal_lostfound_write_comment: "Write comment...",
        
        modal_details_title: "Item Details",
        modal_details_close: "Close",
        
        // Login Page
        login_title: "Where is the Train",
        login_subtitle: "ADMIN PORTAL",
        login_email_lbl: "Email Address",
        login_password_lbl: "Password",
        login_remember_me: "Remember me",
        login_submit: "Sign In",
        login_error_invalid: "Invalid credentials.",
        
        // Theme
        theme_light: "Light Theme",
        theme_dark: "Dark Theme",
        language_toggle: "AR",
        users: "users",
        recent_crowd_updates: "Recent Crowd Updates",
        moderate_btn: "Moderate",
        
        alerts: "Disruptions",
        tab_users_title: "User Management",
        tab_users_subtitle: "Manage registered user accounts and modify their system roles or access status.",
        tab_alerts_title: "Service Disruptions",
        tab_alerts_subtitle: "Manage and broadcast active line suspensions or delays to users.",
        status_suspended: "Suspended",
        status_active_user: "Active",
        crowd_emptychairs: "Empty Chairs",
        crowd_fullchairs: "Full Chairs",
        crowd_aislecrowded: "Aisle Crowded",
        
        // SweetAlert/Confirmations
        confirm_delete_stop: "Are you sure you want to delete this stop?",
        confirm_delete_train: "Are you sure you want to delete this train and all its scheduled route stops?",
        confirm_delete_trip: "Are you sure you want to delete this trip? All crowd updates and follower records will be deleted!",
        confirm_delete_comment: "Are you sure you want to delete this comment?",
        edit_comment_prompt: "Edit comment content:",
        err_comment_empty: "Comment content cannot be empty.",
        app_configs: "App Configurations",
        cities: "Cities",
        add_city: "Add City",
        search_city_placeholder: "Search by name...",
        table_col_name_ar: "Name (Arabic)",
        table_col_name_en: "Name (English)",
        table_col_stops_count: "Stops Count",
        loading_cities: "Loading cities...",
        no_cities: "No cities found. Click 'Add City' to create one.",
        modal_city_add_title: "Add City",
        modal_city_edit_title: "Edit City",
        modal_city_name_ar: "Name (Arabic)",
        modal_city_name_en: "Name (English)",
        modal_save_city: "Save City",
        confirm_delete_city: "Are you sure you want to delete this city? This will unassign it from any stops.",
        modal_stop_city: "City",
        modal_stop_select_city: "-- Select City --",
        tab_cities_title: "Manage Cities",
        tab_cities_subtitle: "Configure cities used for stop/station locations.",
        governments: "Governments",
        add_government: "Add Government",
        search_government_placeholder: "Search by name...",
        table_col_governorate: "Governorate",
        loading_governments: "Loading governments...",
        no_governments: "No governments found. Click 'Add Government' to create one.",
        modal_government_add_title: "Add Government",
        modal_government_edit_title: "Edit Government",
        modal_government_name_ar: "Name (Arabic)",
        modal_government_name_en: "Name (English)",
        modal_save_government: "Save Government",
        confirm_delete_government: "Are you sure you want to delete this government? This will fail if there are cities linked to it.",
        modal_city_governorate: "Governorate",
        modal_city_select_governorate: "-- Select Governorate --",
        tab_governments_title: "Manage Governments",
        tab_governments_subtitle: "Configure governorates in Egypt.",
        system_settings: "System",
        system_settings_title: "System Settings",
        setting_lf_posts_lbl: "Lost & Found Posts Auto-Publish",
        setting_lf_posts_desc: "If enabled, commuter posts will publish instantly without admin review.",
        setting_lf_comments_lbl: "Lost & Found Comments Auto-Publish",
        setting_lf_comments_desc: "If enabled, comments will publish instantly without admin review.",
        setting_live_updates_lbl: "Trip Live Updates Auto-Publish",
        setting_live_updates_desc: "If enabled, train GPS/crowd status updates will publish instantly.",
        setting_live_updates_removal_lbl: "Allow direct removal of live update posts",
        setting_live_updates_removal_desc: "When enabled, a user's own \"remove post\" request is immediately processed. When disabled, the request is queued and requires admin approval.",
        save_settings_btn: "Save Settings",
        settings_saved_success: "System settings saved successfully!",
        review_updates_btn: "Review Live Updates",
        modal_pending_updates_title: "Moderate Live Updates",
        approve_update_success: "Live update approved successfully!",
        delete_update_success: "Live update rejected and deleted!",
        confirm_delete_update: "Are you sure you want to delete/reject this live update?",
        tab_system_title: "System Settings",
        tab_system_subtitle: "Configure system flags and moderation settings.",
        upload_photo: "Upload photo",
        remove_avatar: "Remove",
        modal_edit_avatar_title: "Change profile picture",
        status_tags: "Status Tags",
        crowd_levels: "Crowd Levels",
        tab_status_tags_title: "Configure Status Tags",
        tab_status_tags_subtitle: "Add, modify, or delete configurable status tags for train updates.",
        tab_crowd_levels_title: "Configure Crowd Levels",
        tab_crowd_levels_subtitle: "Add, modify, or delete configurable crowd levels for train updates.",
        search_status_tags_placeholder: "Search status tags...",
        search_crowd_levels_placeholder: "Search crowd levels...",
        add_status_tag: "Add Status Tag",
        add_crowd_level: "Add Crowd Level",
        modal_lookup_type: "Type",
        modal_lookup_code: "Code",
        modal_lookup_name_ar: "Name (Arabic)",
        modal_lookup_name_en: "Name (English)",
        modal_save_lookup: "Save Lookup",
        table_col_type: "Type",
        table_col_code: "Code",
        table_col_name_ar: "Name (Arabic)",
        table_col_name_en: "Name (English)",
        confirm_delete_lookup: "Are you sure you want to delete this lookup value?",
        notifications: "Notifications",
        mark_all_read: "Mark all read",
        no_notifications: "No notifications yet",
        my_profile: "My Profile"
    },
    ar: {
        // App Portal Titles
        dashboard: "لوحة التحكم",
        trains: "القطارات",
        stops: "المحطات",
        trips: "الرحلات",
        suggestions: "الاقتراحات",
        lostfound: "المفقودات والموجودات",
        railway_paths: "مسارات السكك الحديدية",
        tab_railway_paths_title: "إدارة مسارات السكك الحديدية",
        tab_railway_paths_subtitle: "تعريف المسارات الفعلية لخطوط السكك الحديدية بين المحطات باستخدام GeoJSON.",
        live_updates: "التحديثات المباشرة",
        tab_updates_title: "إدارة التحديثات المباشرة",
        tab_updates_subtitle: "الموافقة على تحديثات القطارات المباشرة المقدمة من المستخدمين وإدارة طلبات الإزالة.",
        pendingApproval: "معلق للموافقة",
        noPendingUpdates: "لا توجد تحديثات مباشرة معلقة.",
        removalRequests: "طلبات الإزالة",
        noRemovalRequests: "لا توجد طلبات إزالة نشطة.",
        reject: "رفض",
        approve: "موافقة",
        denyRemoval: "رفض الإزالة",
        confirmRemoval: "تأكيد الإزالة",
        deny_removal_success: "تم رفض طلب إزالة التحديث المباشر.",
        
        tab_dashboard_title: "لوحة التحكم",
        tab_dashboard_subtitle: "نظرة عامة على أنشطة المنصة والإحصاءات.",
        tab_trains_title: "إدارة القطارات",
        tab_trains_subtitle: "تهيئة القطارات، والمحطات، والمسارات، ومواقيت الجدولة.",
        tab_stops_title: "إدارة المحطات",
        tab_stops_subtitle: "إضافة، تعديل أو حذف محطات السكك الحديدية وإحداثيات GPS.",
        tab_trips_title: "رحلات القطار",
        tab_trips_subtitle: "متابعة وجدولة رحلات القطارات اليومية والتأخيرات والحالات.",
        tab_suggestions_title: "اقتراحات المستخدمين",
        tab_suggestions_subtitle: "مراجعة مسارات القطارات المقترحة المقدمة من الركاب.",
        tab_lostfound_title: "إدارة المفقودات والموجودات",
        tab_lostfound_subtitle: "مراجعة بلاغات الركاب، وإدارة انتقال الحالات، ومراقبة التعليقات.",
        tab_train_details_title: "تفاصيل القطار",
        tab_train_details_subtitle: "عرض مواصفات ومسارات ومتابعي هذا القطار.",
        tab_trip_details_title: "تفاصيل الرحلة",
        tab_trip_details_subtitle: "عرض جدول الرحلة والتحديثات ومتابعي هذه الرحلة.",
        back_to_list: "العودة للقائمة",

        // Stats Cards
        stat_trains_lbl: "القطارات",
        stat_stops_lbl: "المحطات",
        stat_trips_lbl: "الرحلات النشطة",
        stat_suggestions_lbl: "الاقتراحات المعلقة",
        
        // Buttons
        add_train: "إضافة قطار",
        add_stop: "إضافة محطة",
        schedule_trip: "جدولة رحلة",
        logout: "تسجيل الخروج",
        admin_portal: "بوابة المسؤولين",
        
        // Search & Filters
        search_train_placeholder: "البحث بالرقم أو الاسم...",
        search_stop_placeholder: "البحث بالكود أو الاسم...",
        search_lostfound_placeholder: "البحث في العنوان أو القطار...",
        filter_all_types: "جميع الأنواع",
        filter_all_statuses: "جميع الحالات",
        type_lost: "مفقود",
        type_found: "موجود",
        
        // Table Headers
        table_col_code: "الكود",
        table_col_station_name: "اسم المحطة",
        table_col_city: "المدينة",
        table_col_lat: "خط العرض",
        table_col_lng: "خط الطول",
        table_col_description: "الوصف",
        table_col_actions: "الإجراءات",
        table_col_train_no: "رقم القطار",
        table_col_train_name: "اسم القطار",
        table_col_stops_seq: "مسار المحطات",
        table_col_status: "الحالة",
        table_col_train: "القطار",
        table_col_trip_date: "تاريخ الرحلة",
        table_col_actual_dep: "المغادرة الفعلية",
        table_col_actual_arr: "الوصول الفعلي",
        table_col_followers: "المتابعون",
        table_col_name: "الاسم",
        table_col_proposed_route: "المسار المقترح",
        table_col_suggested_by: "اقترح بواسطة",
        table_col_date: "التاريخ",
        table_col_type: "النوع",
        table_col_title: "العنوان",
        table_col_author: "الناشر",
        table_col_comments: "التعليقات",
        
        // Status maps
        status_scheduled: "مجدول",
        status_departed: "غادر",
        status_intransit: "في الطريق",
        status_arrived: "وصل",
        status_cancelled: "ملغى",
        status_delayed: "متأخر",
        status_active: "نشط",
        status_inactive: "غير نشط",

        // Crowd tags
        status_ontime: "في الموعد",
        status_crowded: "مزدحم",
        status_empty: "فارغ",
        status_atstation: "في المحطة",
        status_new: "جديد",
        status_published: "منشور",
        status_rejected: "مرفوض",
        status_closed: "مغلق",
        
        // Messages
        loading_updates: "جاري تحميل التحديثات الأخيرة...",
        no_updates: "لا توجد تحديثات منشورة اليوم.",
        api_connected: "متصل بالخادم",
        loading_trains: "جاري تحميل القطارات...",
        no_trains: "لم يتم العثور على قطارات. اضغط على 'إضافة قطار' لإنشاء واحد.",
        view_sequence: "عرض المسار",
        loading_stops: "جاري تحميل المحطات...",
        no_stops: "لم يتم العثور على محطات. اضغط على 'إضافة محطة' لإنشاء واحدة.",
        loading_trips: "جاري تحميل الرحلات المجدولة...",
        no_trips: "لا توجد رحلات مجدولة. اضغط على 'جدولة رحلة' لإنشاء واحدة.",
        loading_suggestions: "جاري تحميل الاقتراحات المعلقة...",
        no_suggestions: "لا توجد اقتراحات قطار معلقة للمراجعة.",
        loading_lostfound: "جاري تحميل بلاغات المفقودات والموجودات...",
        no_lostfound: "لم يتم العثور على بلاغات مطابقة.",
        no_comments: "لا توجد تعليقات على هذا البلاغ.",
        
        // Profile
        profile_settings_title: "إعدادات الملف الشخصي",
        profile_display_name_lbl: "الاسم المعروض",
        profile_bio_lbl: "نبذة تعريفية",
        profile_avatar_file_lbl: "الصورة الشخصية",
        save_profile_btn: "حفظ الملف الشخصي",
        tab_profile_title: "الملف الشخصي للمشرف",
        tab_profile_subtitle: "عرض وتعديل إعدادات حساب المشرف الخاص بك.",
        
        // Modals
        modal_stop_add_title: "إضافة محطة",
        modal_stop_edit_title: "تعديل المحطة",
        modal_stop_code: "كود المحطة (فريد)",
        modal_stop_name_ar: "الاسم (بالعربية)",
        modal_stop_name_en: "الاسم (بالإنجليزية)",
        modal_stop_city_ar: "المدينة (بالعربية)",
        modal_stop_city_en: "المدينة (بالإنجليزية)",
        modal_stop_desc_ar: "الوصف (بالعربية)",
        modal_stop_desc_en: "الوصف (بالإنجليزية)",
        modal_stop_lat: "خط العرض",
        modal_stop_lng: "خط الطول",
        modal_stop_map_lbl: "خريطة الموقع (اسحب العلامة أو انقر على الخريطة لتحديد الإحداثيات)",
        modal_cancel: "إلغاء",
        modal_save_stop: "حفظ المحطة",
        
        modal_train_add_title: "إضافة مسار قطار",
        modal_train_edit_title: "تعديل مسار قطار",
        modal_train_number: "رقم القطار (فريد)",
        modal_train_name_en: "اسم القطار (بالإنجليزية)",
        modal_train_name_ar: "اسم القطار (بالعربية)",
        modal_train_desc_en: "الوصف (بالإنجليزية)",
        modal_train_desc_ar: "الوصف (بالعربية)",
        modal_train_route_stops: "جدول محطات المسار",
        modal_train_add_stop: "إضافة محطة إلى المسار",
        modal_train_save: "حفظ القطار",
        modal_train_seq: "الترتيب",
        modal_train_station: "المحطة",
        modal_train_arr: "وقت الوصول",
        modal_train_dep: "وقت المغادرة",
        
        modal_trip_title: "جدولة رحلة جديدة",
        modal_trip_train: "اختر القطار",
        modal_trip_date: "تاريخ الرحلة",
        modal_trip_status: "الحالة",
        modal_trip_submit: "جدولة",
        
        modal_trip_update_title: "تحديث حالة الرحلة",
        modal_trip_actual_dep: "وقت المغادرة الفعلي (اختياري)",
        modal_trip_actual_arr: "وقت الوصول الفعلي (اختياري)",
        modal_trip_update_btn: "تحديث الحالة",
        
        modal_suggestion_title: "مراجعة اقتراح القطار",
        modal_suggestion_train: "القطار",
        modal_suggestion_route: "المسار المقترح",
        modal_suggestion_author: "اقترح بواسطة",
        modal_suggestion_action: "الإجراء",
        modal_suggestion_approve: "موافقة",
        modal_suggestion_reject: "رفض",
        modal_suggestion_notes: "ملاحظات / تعليقات للمستخدم",
        modal_suggestion_submit: "تقديم المراجعة",
        
        modal_lostfound_title: "إدارة بلاغ المفقودات والموجودات",
        modal_lostfound_details: "تفاصيل البلاغ",
        confirm_delete_comment: "هل أنت متأكد من رغبتك في حذف هذا التعليق؟",
        edit_comment_prompt: "تعديل محتوى التعليق:",
        err_comment_empty: "لا يمكن أن يكون محتوى التعليق فارغاً.",
        app_configs: "إعدادات التطبيق",
        cities: "المدن",
        add_city: "إضافة مدينة",
        search_city_placeholder: "البحث بالاسم...",
        table_col_name_ar: "الاسم (بالعربية)",
        table_col_name_en: "الاسم (بالإنجليزية)",
        table_col_stops_count: "عدد المحطات",
        loading_cities: "جاري تحميل المدن...",
        no_cities: "لم يتم العثور على مدن. اضغط على 'إضافة مدينة' لإنشاء واحدة.",
        modal_city_add_title: "إضافة مدينة",
        modal_city_edit_title: "تعديل مدينة",
        modal_city_name_ar: "الاسم (بالعربية)",
        modal_city_name_en: "الاسم (بالإنجليزية)",
        modal_save_city: "حفظ المدينة",
        confirm_delete_city: "هل أنت متأكد من رغبتك في حذف هذه المدينة؟ سيؤدي ذلك إلى إلغاء تعيينها من أي محطات.",
        modal_stop_city: "المدينة",
        modal_stop_select_city: "-- اختر المدينة --",
        tab_cities_title: "إدارة المدن",
        tab_cities_subtitle: "تهيئة وإدارة المدن المستخدمة لتصنيف مواقع المحطات.",
        governments: "المحافظات",
        add_government: "إضافة محافظة",
        search_government_placeholder: "البحث بالاسم...",
        table_col_governorate: "المحافظة",
        loading_governments: "جاري تحميل المحافظات...",
        no_governments: "لم يتم العثور على محافظات. اضغط على 'إضافة محافظة' لإنشاء واحدة.",
        modal_government_add_title: "إضافة محافظة",
        modal_government_edit_title: "تعديل محافظة",
        modal_government_name_ar: "الاسم (بالعربية)",
        modal_government_name_en: "الاسم (بالإنجليزية)",
        modal_save_government: "حفظ المحافظة",
        confirm_delete_government: "هل أنت متأكد من رغبتك في حذف هذه المحافظة؟ سيفشل الإجراء إذا كانت هناك مدن مرتبطة بها.",
        modal_city_governorate: "المحافظة",
        modal_city_select_governorate: "-- اختر المحافظة --",
        tab_governments_title: "إدارة المحافظات",
        tab_governments_subtitle: "تهيئة وإدارة المحافظات في جمهورية مصر العربية.",
        system_settings: "النظام",
        system_settings_title: "إعدادات النظام",
        setting_lf_posts_lbl: "النشر التلقائي لبلاغات المفقودات",
        setting_lf_posts_desc: "إذا تم تفعيله، سيتم نشر بلاغات الركاب فوراً دون مراجعة المسؤول.",
        setting_lf_comments_lbl: "النشر التلقائي لتعليقات المفقودات",
        setting_lf_comments_desc: "إذا تم تفعيله، سيتم نشر التعليقات فوراً دون مراجعة المسؤول.",
        setting_live_updates_lbl: "النشر التلقائي لتحديثات الرحلات",
        setting_live_updates_desc: "إذا تم تفعيله، سيتم نشر تحديثات موقع وحالة القطار فوراً.",
        setting_live_updates_removal_lbl: "السماح بالإزالة المباشرة لتحديثات الرحلات",
        setting_live_updates_removal_desc: "إذا تم تفعيله، سيتم معالجة طلب المستخدم لحذف تحديثه فوراً. إذا تم تعطيله، سيتم إدراج الطلب في قائمة الانتظار للمراجعة والموافقة.",
        save_settings_btn: "حفظ الإعدادات",
        settings_saved_success: "تم حفظ إعدادات النظام بنجاح!",
        review_updates_btn: "مراجعة التحديثات النشطة",
        modal_pending_updates_title: "إدارة تحديثات الرحلات",
        approve_update_success: "تمت الموافقة على التحديث بنجاح!",
        delete_update_success: "تم رفض وحذف التحديث بنجاح!",
        confirm_delete_update: "هل أنت متأكد من رغبتك في حذف/رفض هذا التحديث؟",
        tab_system_title: "إعدادات النظام",
        tab_system_subtitle: "تهيئة الخصائص العامة وسياسات النشر التلقائي والتنبيهات.",
        modal_lostfound_post_title: "العنوان",
        modal_lostfound_post_desc: "الوصف",
        modal_lostfound_post_type: "النوع",
        modal_lostfound_post_train: "رقم القطار",
        modal_lostfound_post_contact: "معلومات الاتصال",
        modal_lostfound_post_status: "حالة البلاغ",
        modal_lostfound_save_btn: "حفظ التغييرات",
        modal_lostfound_comments: "إدارة التعليقات",
        modal_lostfound_admin_comment: "إضافة تعليق المسؤول",
        modal_lostfound_write_comment: "اكتب تعليقاً...",
        
        modal_details_title: "تفاصيل العنصر",
        modal_details_close: "إغلاق",
        
        // Login Page
        login_title: "أين القطار",
        login_subtitle: "بوابة المسؤولين",
        login_email_lbl: "البريد الإلكتروني",
        login_password_lbl: "كلمة المرور",
        login_remember_me: "تذكرني",
        login_submit: "تسجيل الدخول",
        login_error_invalid: "بيانات الاعتماد غير صالحة.",
        
        // Theme
        theme_light: "المظهر الفاتح",
        theme_dark: "المظهر الداكن",
        language_toggle: "EN",
        users: "مستخدمين",
        recent_crowd_updates: "تحديثات الازدحام الأخيرة",
        moderate_btn: "إدارة البلاغ",
        
        alerts: "الأعطال",
        tab_users_title: "إدارة المستخدمين",
        tab_users_subtitle: "إدارة حسابات المستخدمين المسجلين وتعديل أدوارهم أو حالة وصولهم للنظام.",
        tab_alerts_title: "أعطال الخدمة",
        tab_alerts_subtitle: "إدارة وبث بلاغات توقف الخطوط أو التأخيرات النشطة للمستخدمين.",
        status_suspended: "موقوف",
        status_active_user: "نشط",
        crowd_emptychairs: "كراسي شاغرة",
        crowd_fullchairs: "كراسي ممتلئة",
        crowd_aislecrowded: "الممرات مزدحمة",
        
        // SweetAlert/Confirmations
        confirm_delete_stop: "هل أنت متأكد من رغبتك في حذف هذه المحطة؟",
        confirm_delete_train: "هل أنت متأكد من حذف هذا القطار وجميع محطات مساره المجدولة؟",
        confirm_delete_trip: "هل أنت متأكد من حذف هذه الرحلة؟ سيتم حذف جميع تحديثات الازدحام وسجلات المتابعين!",
        confirm_delete_post: "هل أنت متأكد من حذف هذا البلاغ وجميع تعليقاته؟ لا يمكن التراجع عن هذا الإجراء.",
        confirm_delete_comment: "هل أنت متأكد من حذف هذا التعليق؟",
        edit_comment_prompt: "تعديل محتوى التعليق:",
        err_comment_empty: "لا يمكن أن يكون محتوى التعليق فارغاً.",
        upload_photo: "تحميل صورة",
        remove_avatar: "إزالة",
        modal_edit_avatar_title: "تغيير صورة الملف الشخصي",
        status_tags: "علامات الحالة",
        crowd_levels: "مستويات الازدحام",
        tab_status_tags_title: "تهيئة علامات الحالة",
        tab_status_tags_subtitle: "إضافة، تعديل أو حذف علامات الحالة القابلة للتهيئة لتحديثات القطار.",
        tab_crowd_levels_title: "تهيئة مستويات الازدحام",
        tab_crowd_levels_subtitle: "إضافة، تعديل أو حذف مستويات الازدحام القابلة للتهيئة لتحديثات القطار.",
        search_status_tags_placeholder: "بحث عن علامات الحالة...",
        search_crowd_levels_placeholder: "بحث عن مستويات الازدحام...",
        add_status_tag: "إضافة علامة حالة",
        add_crowd_level: "إضافة مستوى ازدحام",
        modal_lookup_code: "الكود",
        modal_lookup_name_ar: "الاسم (بالعربية)",
        modal_lookup_name_en: "الاسم (بالإنجليزية)",
        table_col_code: "الكود",
        table_col_name_ar: "الاسم (بالعربية)",
        table_col_name_en: "الاسم (بالإنجليزية)",
        confirm_delete_status_tag: "هل أنت متأكد من حذف علامة الحالة هذه؟",
        confirm_delete_crowd_level: "هل أنت متأكد من حذف مستوى الازدحام هذا؟",
        notifications: "التنبيهات",
        mark_all_read: "تحديد الكل كمقروء",
        no_notifications: "لا توجد تنبيهات بعد",
        my_profile: "ملفي الشخصي"
    }
};

function t(key) {
    const lang = state.language || 'en';
    return TRANSLATIONS[lang]?.[key] || TRANSLATIONS['en']?.[key] || key;
}

function getLookupName(type, code) {
    if (!code) return '';
    let match = null;
    if (type.toLowerCase() === 'statustag') {
        match = state.statusTags.find(l => l.code.toLowerCase() === code.toLowerCase());
    } else if (type.toLowerCase() === 'crowdlevel') {
        match = state.crowdLevels.find(l => l.code.toLowerCase() === code.toLowerCase());
    }
    if (match) {
        return state.language === 'ar' ? match.nameAr : match.nameEn;
    }
    const key = type === 'StatusTag' ? `status_${code.toLowerCase()}` : `crowd_${code.toLowerCase()}`;
    return t(key) || code;
}

function applyLocalization() {
    const lang = state.language;
    document.documentElement.lang = lang;
    document.documentElement.dir = lang === 'ar' ? 'rtl' : 'ltr';
    
    if (lang === 'ar') {
        document.body.classList.add('lang-ar');
    } else {
        document.body.classList.remove('lang-ar');
    }

    // Update language toggle button tooltip and text if present
    const langBtn = document.getElementById('lang-toggle-btn');
    if (langBtn) {
        langBtn.title = lang === 'ar' ? 'English' : 'العربية';
    }
    const langToggleText = document.getElementById('lang-toggle-text');
    if (langToggleText) {
        langToggleText.textContent = lang === 'ar' ? 'English' : 'العربية';
    }

    // Update theme toggle text / tooltip
    const isLight = document.body.classList.contains('light-theme');
    const themeBtn = document.getElementById('theme-toggle-btn');
    if (themeBtn) {
        themeBtn.title = isLight ? t('theme_dark') : t('theme_light');
    }
    const themeTextEl = document.querySelector('#theme-toggle-btn span');
    if (themeTextEl) {
        themeTextEl.textContent = isLight ? t('theme_dark') : t('theme_light');
    }

    // Translate all elements with data-i18n
    document.querySelectorAll('[data-i18n]').forEach(el => {
        const key = el.getAttribute('data-i18n');
        const translation = t(key);
        if (translation) {
            if (el.tagName === 'INPUT' || el.tagName === 'TEXTAREA') {
                el.placeholder = translation;
            } else {
                const icon = el.querySelector('i');
                if (icon) {
                    let textNode = Array.from(el.childNodes).find(node => node.nodeType === Node.TEXT_NODE);
                    if (textNode) {
                        textNode.textContent = ' ' + translation;
                    } else {
                        el.innerHTML = '';
                        el.appendChild(icon);
                        el.appendChild(document.createTextNode(' ' + translation));
                    }
                } else {
                    el.textContent = translation;
                }
            }
        }
    });

    // Refresh layout of current tab headers
    if (state.token) {
        const titleEl = document.getElementById('tab-title');
        const subtitleEl = document.getElementById('tab-subtitle');
        if (titleEl && subtitleEl) {
            titleEl.textContent = t(`tab_${state.activeTab}_title`);
            subtitleEl.textContent = t(`tab_${state.activeTab}_subtitle`);
        }
    }
}

function toggleLanguage() {
    state.language = state.language === 'en' ? 'ar' : 'en';
    localStorage.setItem('witt_admin_language', state.language);
    applyLocalization();
    
    // Refresh table structures
    if (state.token) {
        switchTab(state.activeTab);
    }
}

// ==========================================================================
// API CLIENT WRAPPER (Handles Auth Headers and Session Expiry)
// ==========================================================================
async function apiFetch(endpoint, options = {}) {
    if (!state.token) {
        logout();
        throw new Error('Not authenticated');
    }

    const headers = {
        'Authorization': `Bearer ${state.token}`,
        'Cache-Control': 'no-cache',
        'Pragma': 'no-cache',
        ...options.headers
    };
    if (!(options.body instanceof FormData)) {
        headers['Content-Type'] = 'application/json';
    }

    let url = `${API_URL}${endpoint}`;
    if (options.method === 'GET' || !options.method) {
        const separator = url.includes('?') ? '&' : '?';
        url = `${url}${separator}_t=${Date.now()}`;
    }

    const res = await fetch(url, {
        ...options,
        headers
    });

    if (res.status === 401 || res.status === 403) {
        alert('Session expired or unauthorized access. Logging out.');
        logout();
        throw new Error('Unauthorized');
    }

    // Try parsing JSON
    let data;
    try {
        data = await res.json();
    } catch (e) {
        data = null;
    }

    if (!res.ok) {
        const errorMsg = data?.message || data?.error || `Request failed with status ${res.status}`;
        throw new Error(errorMsg);
    }

    // Unpack Result<T> structure if present
    if (data && data.hasOwnProperty('isSuccess')) {
        if (!data.isSuccess) {
            throw new Error(data.error || 'Operation failed.');
        }
        return data.data;
    }

    return data;
}

// ==========================================================================
// AUTHENTICATION LOGIC
// ==========================================================================
document.getElementById('login-form').addEventListener('submit', async (e) => {
    e.preventDefault();
    const email = document.getElementById('email').value;
    const password = document.getElementById('password').value;
    const rememberMe = document.getElementById('remember-me').checked;
    const errorEl = document.getElementById('login-error');
    const errorTextEl = document.getElementById('login-error-text');

    errorEl.classList.add('hidden');

    try {
        const res = await fetch(`${API_URL}/api/auth/login`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ email, password, rememberMe })
        });

        const data = await res.json();
        if (!res.ok || !data.isSuccess) {
            throw new Error(data.error || 'Login failed.');
        }

        const authResult = data.data;
        state.token = authResult.accessToken;
        
        try {
            // Test call to verify Admin role
            await apiFetch('/api/admin/stops');
        } catch (err) {
            state.token = '';
            throw new Error('Access denied. Admin role required.');
        }

        // Save session
        state.user = {
            displayName: authResult.displayName,
            email: authResult.email
        };
        const storage = rememberMe ? localStorage : sessionStorage;
        storage.setItem('witt_admin_token', state.token);
        storage.setItem('witt_admin_user', JSON.stringify(state.user));

        initPortal();
    } catch (err) {
        errorTextEl.textContent = err.message;
        errorEl.classList.remove('hidden');
    }
});

document.getElementById('logout-btn').addEventListener('click', logout);

function logout() {
    state.token = '';
    state.user = null;
    localStorage.removeItem('witt_admin_token');
    localStorage.removeItem('witt_admin_user');
    sessionStorage.removeItem('witt_admin_token');
    sessionStorage.removeItem('witt_admin_user');
    document.getElementById('main-layout').classList.add('hidden');
    document.getElementById('login-container').classList.remove('hidden');
}

// ==========================================================================
// PORTAL ROUTER & MAIN INITIALIZER
// ==========================================================================
window.addEventListener('DOMContentLoaded', () => {
    initTheme();
    bindThemeToggleListener();
    bindLangToggleListener();
    applyLocalization();
    setupDropdownListeners();

    if (state.token && state.user) {
        initPortal();
    } else {
        logout();
    }
});

function initPortal() {
    document.getElementById('login-container').classList.add('hidden');
    document.getElementById('main-layout').classList.remove('hidden');
    
    // Set Profile UI
    const adminName = document.getElementById('admin-name');
    const dropdownAdminName = document.getElementById('dropdown-admin-name');
    const adminEmail = document.getElementById('admin-email');
    if (adminName) adminName.textContent = state.user.displayName;
    if (dropdownAdminName) dropdownAdminName.textContent = state.user.displayName;
    if (adminEmail) adminEmail.textContent = state.user.email;

    // Load Profile to sync sidebar avatar
    loadProfile();
    initLookups();

    // Load initial tab
    switchTab('dashboard');

    // Fetch suggestion counts periodically for badge
    fetchSuggestionCountBadge();
    setInterval(fetchSuggestionCountBadge, 30000);

    // Fetch active disruption banner on load and check every minute
    fetchAndRenderActiveBanner();
    setInterval(fetchAndRenderActiveBanner, 60000);

    // Fetch notifications on load and check periodically
    fetchNotifications();
    setInterval(fetchNotifications, 30000);
}

// Tab Switching Routing
function setupNavigationListeners() {
    const navItems = document.querySelectorAll('.menu-item, .menu-subitem');
    navItems.forEach(item => {
        item.addEventListener('click', (e) => {
            if (item.classList.contains('menu-parent')) return; // handled by onclick/chevron
            e.preventDefault();
            const tab = item.getAttribute('data-tab');
            if (tab) {
                switchTab(tab);
            }
        });
    });
}

function toggleSubmenu(e) {
    e.preventDefault();
    const parent = e.currentTarget.closest('.menu-group');
    const subitems = parent.querySelector('.menu-subitems');
    if (subitems.classList.contains('hidden')) {
        subitems.classList.remove('hidden');
        parent.classList.add('open');
    } else {
        subitems.classList.add('hidden');
        parent.classList.remove('open');
    }
}
window.toggleSubmenu = toggleSubmenu; // Make globally accessible

// Initial call to setup navigation listeners
setupNavigationListeners();

function switchTab(tabName) {
    if (!state.token) return; // Prevent API errors when not authenticated

    state.activeTab = tabName;
    
    // Update menu UI
    const navItems = document.querySelectorAll('.menu-item, .menu-subitem');
    navItems.forEach(item => {
        if (item.getAttribute('data-tab') === tabName) {
            item.classList.add('active');
        } else {
            item.classList.remove('active');
        }
    });

    // Toggle panels
    document.querySelectorAll('.tab-panel').forEach(panel => {
        panel.classList.add('hidden');
    });
    document.getElementById(`tab-${tabName}`).classList.remove('hidden');

    // Update Headers
    const titleEl = document.getElementById('tab-title');
    const subtitleEl = document.getElementById('tab-subtitle');

    if (titleEl && subtitleEl) {
        titleEl.textContent = t(`tab_${tabName.replace(/-/g, '_')}_title`);
        subtitleEl.textContent = t(`tab_${tabName.replace(/-/g, '_')}_subtitle`);
    }

    switch (tabName) {
        case 'dashboard':
            loadDashboard();
            break;
        case 'trains':
            loadTrains();
            break;
        case 'stops':
            loadStops();
            break;
        case 'trips':
            loadTrips();
            break;
        case 'railway-paths':
            loadRailwayPaths();
            break;
        case 'suggestions':
            loadSuggestions();
            break;
        case 'lostfound':
            loadLostFoundPosts();
            break;
        case 'updates':
            loadLiveUpdatesModeration();
            break;
        case 'users':
            loadUsers();
            break;
        case 'alerts':
            loadDisruptions();
            break;
        case 'cities':
            loadCities();
            break;
        case 'governments':
            loadGovernments();
            break;
        case 'system':
            loadSystemSettings();
            break;
        case 'status-tags':
            loadStatusTags();
            break;
        case 'crowd-levels':
            loadCrowdLevels();
            break;
        case 'profile':
            loadProfile();
            break;
    }
}

// ==========================================================================
// 📊 DASHBOARD LOGIC
// ==========================================================================
async function loadDashboard() {
    try {
        const stats = await apiFetch('/api/dashboard');
        
        document.getElementById('stat-trains').textContent = stats.totalTrains;
        document.getElementById('stat-stops').textContent = stats.totalUsers; // Note: dashboard returns totalUsers in second spot
        document.getElementById('stat-trips').textContent = stats.activeTripsToday;
        
        // Fetch pending suggestions directly from admin endpoint for accuracy
        const suggestions = await apiFetch('/api/admin/suggestions');
        document.getElementById('stat-suggestions').textContent = suggestions.length;
        document.getElementById('stat-stops').textContent = state.stops.length || '-'; // Update stops count
        
        // Cache stats to stops count if stops aren't loaded yet
        if (state.stops.length === 0) {
            const stopsData = await apiFetch('/api/admin/stops');
            state.stops = stopsData;
            document.getElementById('stat-stops').textContent = stopsData.length;
        }

        // Render recent updates feed
        const feedEl = document.getElementById('dashboard-updates-feed');
        feedEl.innerHTML = '';

        if (!stats.recentUpdates || stats.recentUpdates.length === 0) {
            feedEl.innerHTML = `<p class="loading-text">${t('no_updates')}</p>`;
            return;
        }

        stats.recentUpdates.forEach(update => {
            const timeStr = new Date(update.createdAt).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
            const item = document.createElement('div');
            item.className = 'update-feed-item';
            
            // Format tag
            let tagHtml = '';
            if (update.statusTag) {
                const tagText = getLookupName('StatusTag', update.statusTag);
                tagHtml = `<span class="status-pill ${update.statusTag.toLowerCase()}">${tagText}</span>`;
            }
            
            let crowdHtml = '';
            if (update.crowdState) {
                const crowdText = getLookupName('CrowdLevel', update.crowdState);
                crowdHtml = `<span class="status-pill crowd-${update.crowdState.toLowerCase()}">${crowdText}</span>`;
            }
            
            item.innerHTML = `
                <div class="update-header">
                    <span class="update-author"><i class="fa-regular fa-user"></i> ${update.authorName}</span>
                    <span class="update-time">${timeStr}</span>
                </div>
                <div class="update-content">${update.content}</div>
                <div class="update-meta">
                    ${tagHtml}
                    ${crowdHtml}
                    ${update.latitude ? `<span><i class="fa-solid fa-location-dot"></i> GPS: ${update.latitude.toFixed(4)}, ${update.longitude.toFixed(4)}</span>` : ''}
                </div>
            `;
            feedEl.appendChild(item);
        });

    } catch (err) {
        console.error('Failed to load dashboard:', err);
    }
}

async function fetchSuggestionCountBadge() {
    if (!state.token) return;
    try {
        const suggestions = await apiFetch('/api/admin/suggestions');
        const count = suggestions.length;
        const badge = document.querySelector('.suggestion-count');
        if (count > 0) {
            badge.textContent = count;
            badge.classList.remove('hidden');
        } else {
            badge.classList.add('hidden');
        }
    } catch (e) {
        console.error('Failed to fetch suggestion count badge');
    }
}

// ==========================================================================
// 📍 STOPS CRUD LOGIC
// ==========================================================================
async function loadStops() {
    const tableBody = document.querySelector('#stops-table tbody');
    tableBody.innerHTML = `<tr><td colspan="7" class="loading-cell">${t('loading_stops')}</td></tr>`;
    
    try {
        const stops = await apiFetch('/api/admin/stops');
        state.stops = stops; // Update cache
        tableBody.innerHTML = '';

        if (stops.length === 0) {
            tableBody.innerHTML = `<tr><td colspan="7" class="no-data-cell">${t('no_stops')}</td></tr>`;
            return;
        }

        stops.forEach(stop => {
            const tr = document.createElement('tr');
            
            const stopName = state.language === 'ar' ? (stop.nameAr || stop.nameEn) : (stop.nameEn || stop.nameAr);
            const stopCity = state.language === 'ar' ? (stop.cityAr || stop.cityEn) : (stop.cityEn || stop.cityAr);
            const stopDesc = state.language === 'ar' ? (stop.descriptionAr || stop.descriptionEn) : (stop.descriptionEn || stop.descriptionAr);

            tr.innerHTML = `
                <td><code>${stop.code}</code></td>
                <td style="font-weight: 600; color: white;">${stopName || ''}</td>
                <td>${stopCity || ''}</td>
                <td><code>${stop.latitude.toFixed(4)}</code></td>
                <td><code>${stop.longitude.toFixed(4)}</code></td>
                <td style="color: var(--text-secondary); font-size: 13px;">${stopDesc || ''}</td>
                <td class="actions-column">
                    <button class="action-btn view" onclick="viewStop('${stop.id}')" title="View"><i class="fa-solid fa-eye"></i></button>
                    <button class="action-btn edit" onclick="editStop('${stop.id}')" title="Edit"><i class="fa-solid fa-pencil"></i></button>
                    <button class="action-btn delete" onclick="deleteStop('${stop.id}')" title="Delete"><i class="fa-solid fa-trash"></i></button>
                </td>
            `;
            tableBody.appendChild(tr);
        });
    } catch (err) {
        tableBody.innerHTML = `<tr><td colspan="7" class="no-data-cell" style="color:var(--accent-red)">Error loading stops: ${err.message}</td></tr>`;
    }
}

// Stop Modal Map state
let stopFormMap = null;
let stopFormMarker = null;

function initOrUpdateStopFormMap(lat, lng) {
    const container = document.getElementById('stop-form-map');
    if (!container) return;

    if (typeof L === 'undefined') {
        console.warn('Leaflet is not loaded. Map features are unavailable.');
        container.innerHTML = `
            <div style="display:flex; flex-direction:column; align-items:center; justify-content:center; height:100%; color:var(--text-secondary); font-size:13px; padding:20px; text-align:center; background:rgba(255,255,255,0.01);">
                <i class="fa-solid fa-triangle-exclamation" style="margin-bottom:8px; font-size:24px; color:var(--accent-orange);"></i>
                <span>Map features are currently unavailable.</span>
            </div>
        `;
        return;
    }

    const defaultLat = 30.0626; // Cairo Central default
    const defaultLng = 31.2467;
    
    const initialLat = (lat !== undefined && lat !== null && !isNaN(lat)) ? lat : defaultLat;
    const initialLng = (lng !== undefined && lng !== null && !isNaN(lng)) ? lng : defaultLng;
    
    const latLng = [initialLat, initialLng];

    if (!stopFormMap) {
        // Initialize Map
        stopFormMap = L.map('stop-form-map').setView(latLng, 11);
        
        L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
            maxZoom: 19,
            attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
        }).addTo(stopFormMap);

        // Draggable marker
        stopFormMarker = L.marker(latLng, {
            draggable: true
        }).addTo(stopFormMap);

        // Sync coordinates when dragging the marker
        stopFormMarker.on('dragend', function () {
            const position = stopFormMarker.getLatLng();
            document.getElementById('stop-lat').value = position.lat.toFixed(6);
            document.getElementById('stop-lng').value = position.lng.toFixed(6);
        });

        // Click map to reposition marker and update inputs
        stopFormMap.on('click', function (e) {
            const position = e.latlng;
            stopFormMarker.setLatLng(position);
            document.getElementById('stop-lat').value = position.lat.toFixed(6);
            document.getElementById('stop-lng').value = position.lng.toFixed(6);
        });

        // Listen to input changes to update marker position
        const syncMapFromInputs = () => {
            const latVal = parseFloat(document.getElementById('stop-lat').value);
            const lngVal = parseFloat(document.getElementById('stop-lng').value);
            if (!isNaN(latVal) && !isNaN(lngVal)) {
                const newPos = [latVal, lngVal];
                stopFormMarker.setLatLng(newPos);
                stopFormMap.setView(newPos, stopFormMap.getZoom());
            }
        };

        document.getElementById('stop-lat').addEventListener('input', syncMapFromInputs);
        document.getElementById('stop-lng').addEventListener('input', syncMapFromInputs);
    } else {
        // Map already exists, relocate marker and reset view
        stopFormMarker.setLatLng(latLng);
        stopFormMap.setView(latLng, stopFormMap.getZoom() || 11);
    }

    // Refresh layout size in next event loop since modal transitioning from hidden to visible
    setTimeout(() => {
        if (stopFormMap) {
            stopFormMap.invalidateSize();
        }
    }, 150);
}

async function populateStopCitiesDropdown(selectedCityId = '') {
    const select = document.getElementById('stop-city-id');
    if (!select) return;
    select.innerHTML = `<option value="">${t('modal_stop_select_city')}</option>`;
    try {
        const cities = await apiFetch('/api/admin/cities');
        cities.forEach(city => {
            const opt = document.createElement('option');
            opt.value = city.id;
            opt.textContent = state.language === 'ar' ? city.nameAr : city.nameEn;
            if (city.id === selectedCityId) {
                opt.selected = true;
            }
            select.appendChild(opt);
        });
    } catch (err) {
        console.error('Failed to load cities for stops dropdown:', err);
    }
}

// Stop Modal handlers
async function openStopModal(stop = null) {
    const modal = document.getElementById('stop-modal');
    const title = document.getElementById('stop-modal-title');
    
    document.getElementById('stop-form').reset();
    document.getElementById('stop-id').value = '';

    let lat, lng;
    let selectedCityId = '';

    if (stop) {
        title.textContent = t('modal_stop_edit_title');
        document.getElementById('stop-id').value = stop.id;
        document.getElementById('stop-code').value = stop.code;
        document.getElementById('stop-name-en').value = stop.nameEn || '';
        document.getElementById('stop-name-ar').value = stop.nameAr || '';
        selectedCityId = stop.cityId || '';
        document.getElementById('stop-description-en').value = stop.descriptionEn || '';
        document.getElementById('stop-description-ar').value = stop.descriptionAr || '';
        document.getElementById('stop-lat').value = stop.latitude;
        document.getElementById('stop-lng').value = stop.longitude;
        lat = stop.latitude;
        lng = stop.longitude;
    } else {
        title.textContent = t('modal_stop_add_title');
        // Default to Cairo Central RAMSES coordinates
        lat = 30.0626;
        lng = 31.2467;
        document.getElementById('stop-lat').value = lat;
        document.getElementById('stop-lng').value = lng;
    }

    await populateStopCitiesDropdown(selectedCityId);

    modal.classList.remove('hidden');
    initOrUpdateStopFormMap(lat, lng);
    applyLocalization(); // Translate titles inside
}

function closeStopModal() {
    document.getElementById('stop-modal').classList.add('hidden');
}

async function editStop(id) {
    const stop = state.stops.find(s => s.id === id);
    if (stop) await openStopModal(stop);
}

document.getElementById('stop-form').addEventListener('submit', async (e) => {
    e.preventDefault();
    const id = document.getElementById('stop-id').value;
    const body = {
        nameEn: document.getElementById('stop-name-en').value,
        nameAr: document.getElementById('stop-name-ar').value,
        code: document.getElementById('stop-code').value.toUpperCase(),
        cityId: document.getElementById('stop-city-id').value || null,
        descriptionEn: document.getElementById('stop-description-en').value,
        descriptionAr: document.getElementById('stop-description-ar').value,
        latitude: parseFloat(document.getElementById('stop-lat').value),
        longitude: parseFloat(document.getElementById('stop-lng').value)
    };

    try {
        if (id) {
            await apiFetch(`/api/admin/stops/${id}`, {
                method: 'PUT',
                body: JSON.stringify(body)
            });
        } else {
            await apiFetch('/api/admin/stops', {
                method: 'POST',
                body: JSON.stringify(body)
            });
        }
        closeStopModal();
        loadStops();
    } catch (err) {
        alert(`Error saving stop: ${err.message}`);
    }
});

async function deleteStop(id) {
    if (!confirm(t('confirm_delete_stop'))) return;
    try {
        await apiFetch(`/api/admin/stops/${id}`, { method: 'DELETE' });
        loadStops();
    } catch (err) {
        alert(`Error deleting stop: ${err.message}`);
    }
}

// Search Filter
document.getElementById('stop-search').addEventListener('input', (e) => {
    const query = e.target.value.toLowerCase();
    const rows = document.querySelectorAll('#stops-table tbody tr');
    rows.forEach(row => {
        if (row.cells.length < 2) return;
        const text = row.innerText.toLowerCase();
        if (text.includes(query)) {
            row.style.display = '';
        } else {
            row.style.display = 'none';
        }
    });
});

// ==========================================
// 🚂 TRAINS CRUD LOGIC
// ==========================================
async function loadTrains() {
    const tableBody = document.querySelector('#trains-table tbody');
    tableBody.innerHTML = `<tr><td colspan="6" class="loading-cell">${t('loading_trains')}</td></tr>`;
    
    try {
        const trains = await apiFetch('/api/admin/trains');
        state.trains = trains; // Update cache
        tableBody.innerHTML = '';

        if (trains.length === 0) {
            tableBody.innerHTML = `<tr><td colspan="6" class="no-data-cell">${t('no_trains')}</td></tr>`;
            return;
        }

        trains.forEach(train => {
            const tr = document.createElement('tr');
            
            // Build route stops badge sequence showing start, destination, and dots in between
            let routeHtml = '<div style="display: inline-flex; align-items: center; gap: 8px;">';
            routeHtml += '<div class="stops-badge-list" style="flex-wrap: nowrap; gap: 4px; align-items: center;">';
            if (train.routeStops && train.routeStops.length > 0) {
                const sortedStops = [...train.routeStops].sort((a, b) => a.stopOrder - b.stopOrder);
                if (sortedStops.length <= 2) {
                    sortedStops.forEach((stop, index) => {
                        routeHtml += `<span class="stop-badge-pill">${stop.stopCode}</span>`;
                        if (index < sortedStops.length - 1) {
                            routeHtml += '<i class="fa-solid fa-arrow-right stop-arrow" style="font-size: 8px; margin: 0 2px;"></i>';
                        }
                    });
                } else {
                    const firstStop = sortedStops[0];
                    const lastStop = sortedStops[sortedStops.length - 1];
                    routeHtml += `<span class="stop-badge-pill">${firstStop.stopCode}</span>`;
                    routeHtml += '<i class="fa-solid fa-ellipsis stop-arrow" style="margin: 0 4px; color: var(--text-muted); font-size: 8px;"></i>';
                    routeHtml += `<span class="stop-badge-pill">${lastStop.stopCode}</span>`;
                }
            } else {
                routeHtml += `<span class="stop-badge-pill" style="color: var(--text-muted);">-</span>`;
            }
            routeHtml += '</div>';
            
            if (train.routeStops && train.routeStops.length > 0) {
                routeHtml += `<button class="action-btn view" style="width: 24px; height: 24px; font-size: 11px; margin: 0;" onclick="viewRouteSequence('${train.id}')" title="${t('view_sequence')}"><i class="fa-solid fa-list-ol"></i></button>`;
            }
            routeHtml += '</div>';

            const trainName = state.language === 'ar' ? (train.nameAr || train.nameEn) : (train.nameEn || train.nameAr);

            const activeText = train.isActive ? t('status_active') : t('status_inactive');
            const activeClass = train.isActive ? 'arrived' : 'cancelled';

            tr.innerHTML = `
                <td><strong>${train.trainNumber}</strong></td>
                <td style="font-weight: 600; color: white;">${trainName || ''}</td>
                <td>${routeHtml}</td>
                <td><span class="status-pill ${activeClass}">${activeText}</span></td>
                <td>
                    <button class="btn btn-outline" style="padding: 4px 8px; font-size: 12px; height: auto;" onclick="openTrainFollowersModal('${train.id}', '${train.trainNumber.replace(/'/g, "\\'")}')">
                        <i class="fa-solid fa-users"></i> ${train.followerCount || 0}
                    </button>
                </td>
                <td class="actions-column">
                    <button class="action-btn view" onclick="viewTrain('${train.id}')" title="View"><i class="fa-solid fa-eye"></i></button>
                    <button class="action-btn edit" onclick="editTrain('${train.id}')" title="Edit"><i class="fa-solid fa-pencil"></i></button>
                    <button class="action-btn delete" onclick="deleteTrain('${train.id}')" title="Delete"><i class="fa-solid fa-trash"></i></button>
                </td>
            `;
            tableBody.appendChild(tr);
        });
    } catch (err) {
        tableBody.innerHTML = `<tr><td colspan="6" class="no-data-cell" style="color:var(--accent-red)">Error loading trains: ${err.message}</td></tr>`;
    }
}

function viewRouteSequence(trainId) {
    const train = state.trains.find(t => t.id === trainId);
    if (!train) return;

    const trainName = state.language === 'ar' ? (train.nameAr || train.nameEn) : (train.nameEn || train.nameAr);
    const title = `${t('view_sequence')} - ${train.trainNumber} (${trainName})`;

    const sortedStops = [...train.routeStops].sort((a, b) => a.stopOrder - b.stopOrder);

    let contentHtml = `
        <div style="padding: 10px 0;">
            <table style="width: 100%; border-collapse: collapse; margin-top: 10px;">
                <thead>
                    <tr style="border-bottom: 2px solid var(--accent-purple); color: var(--accent-purple);">
                        <th style="padding: 8px 12px; text-align: ${state.language === 'ar' ? 'right' : 'left'}; font-weight: 600;">Seq</th>
                        <th style="padding: 8px 12px; text-align: ${state.language === 'ar' ? 'right' : 'left'}; font-weight: 600;">Station</th>
                        <th style="padding: 8px 12px; text-align: ${state.language === 'ar' ? 'right' : 'left'}; font-weight: 600;">Arrival</th>
                        <th style="padding: 8px 12px; text-align: ${state.language === 'ar' ? 'right' : 'left'}; font-weight: 600;">Departure</th>
                    </tr>
                </thead>
                <tbody>
    `;

    sortedStops.forEach((stop) => {
        const stopName = state.language === 'ar' ? (stop.stopNameAr || stop.stopNameEn) : (stop.stopNameEn || stop.stopNameAr);
        const arrivalTime = stop.scheduledArrival ? stop.scheduledArrival.substring(0, 5) : '--:--';
        const departureTime = stop.scheduledDeparture ? stop.scheduledDeparture.substring(0, 5) : '--:--';

        contentHtml += `
            <tr style="border-bottom: 1px solid var(--border-color);">
                <td style="padding: 8px 12px; font-weight: 600; color: white;">#${stop.stopOrder}</td>
                <td style="padding: 8px 12px; color: var(--text-primary); font-weight: 500;">${stopName} (${stop.stopCode})</td>
                <td style="padding: 8px 12px; color: var(--text-secondary); font-family: monospace;">${arrivalTime}</td>
                <td style="padding: 8px 12px; color: var(--text-secondary); font-family: monospace;">${departureTime}</td>
            </tr>
        `;
    });

    contentHtml += `
                </tbody>
            </table>
        </div>
    `;

    showDetailsModal(title, contentHtml, false);
}

// Route Stop Row Builder for Train Modal
function addRouteStopRow(stopId = '', arrival = '', departure = '', order = '') {
    const container = document.getElementById('route-stops-list');
    const index = container.children.length;
    const row = document.createElement('tr');
    row.className = 'route-stop-row';

    // Generate Stop options
    let stopOptions = `<option value="">-- ${t('modal_train_station')} --</option>`;
    state.stops.forEach(stop => {
        const stopName = state.language === 'ar' ? (stop.nameAr || stop.nameEn) : (stop.nameEn || stop.nameAr);
        stopOptions += `<option value="${stop.id}" ${stop.id === stopId ? 'selected' : ''}>${stopName} (${stop.code})</option>`;
    });

    row.innerHTML = `
        <td style="text-align: center; vertical-align: middle;"><span class="drag-handle" style="cursor: move; color: var(--text-muted);"><i class="fa-solid fa-bars"></i></span></td>
        <td style="vertical-align: middle;"><input type="number" class="stop-order-input" required value="${order !== '' ? order : index + 1}" min="1" placeholder="Seq" style="width: 100%; padding: 8px; background: var(--bg-main); border: 1px solid var(--border-color); color: white; border-radius: 6px;"></td>
        <td style="vertical-align: middle;">
            <select class="stop-select" required style="width: 100%; padding: 8px; background: var(--bg-main); border: 1px solid var(--border-color); color: white; border-radius: 6px;">
                ${stopOptions}
            </select>
        </td>
        <td style="vertical-align: middle;"><input type="time" class="stop-arr-input" value="${arrival}" style="width: 100%; padding: 8px; background: var(--bg-main); border: 1px solid var(--border-color); color: white; border-radius: 6px;"></td>
        <td style="vertical-align: middle;"><input type="time" class="stop-dep-input" value="${departure}" style="width: 100%; padding: 8px; background: var(--bg-main); border: 1px solid var(--border-color); color: white; border-radius: 6px;"></td>
        <td style="text-align: center; vertical-align: middle;"><button type="button" class="action-btn delete" onclick="this.closest('tr').remove()" style="margin:0;"><i class="fa-solid fa-trash"></i></button></td>
    `;
    container.appendChild(row);
}

// Modal Handlers
async function openTrainModal(train = null) {
    const modal = document.getElementById('train-modal');
    const title = document.getElementById('train-modal-title');
    
    document.getElementById('train-form').reset();
    document.getElementById('train-id').value = '';
    document.getElementById('route-stops-list').innerHTML = '';

    // Verify stops are cached
    if (state.stops.length === 0) {
        state.stops = await apiFetch('/api/admin/stops');
    }

    if (train) {
        title.textContent = t('modal_train_edit_title');
        document.getElementById('train-id').value = train.id;
        document.getElementById('train-number').value = train.trainNumber;
        document.getElementById('train-name-en').value = train.nameEn || '';
        document.getElementById('train-name-ar').value = train.nameAr || '';
        document.getElementById('train-description-en').value = train.descriptionEn || '';
        document.getElementById('train-description-ar').value = train.descriptionAr || '';

        // Populate stops
        train.routeStops.forEach(stop => {
            const formatTime = (ts) => ts ? ts.substring(0, 5) : '';
            addRouteStopRow(stop.stopId, formatTime(stop.scheduledArrival), formatTime(stop.scheduledDeparture), stop.stopOrder);
        });
    } else {
        title.textContent = t('modal_train_add_title');
        addRouteStopRow('', '', '', 1);
        addRouteStopRow('', '', '', 2);
    }

    modal.classList.remove('hidden');
    applyLocalization();
}

function closeTrainModal() {
    document.getElementById('train-modal').classList.add('hidden');
}

function editTrain(id) {
    const train = state.trains.find(t => t.id === id);
    if (train) openTrainModal(train);
}

document.getElementById('train-form').addEventListener('submit', async (e) => {
    e.preventDefault();
    const id = document.getElementById('train-id').value;
    
    // Gather route stops
    const rows = document.querySelectorAll('#route-stops-list .route-stop-row');
    const routeStops = [];
    
    let hasError = false;
    rows.forEach(row => {
        const stopId = row.querySelector('.stop-select').value;
        const stopOrder = parseInt(row.querySelector('.stop-order-input').value);
        const arrivalVal = row.querySelector('.stop-arr-input').value;
        const departureVal = row.querySelector('.stop-dep-input').value;

        if (!stopId) {
            hasError = true;
            return;
        }

        routeStops.push({
            stopId,
            stopOrder,
            scheduledArrival: arrivalVal ? `${arrivalVal}:00` : null,
            scheduledDeparture: departureVal ? `${departureVal}:00` : null
        });
    });

    if (hasError || routeStops.length === 0) {
        alert('Please select valid stations for all route stops.');
        return;
    }

    const body = {
        trainNumber: document.getElementById('train-number').value,
        nameEn: document.getElementById('train-name-en').value,
        nameAr: document.getElementById('train-name-ar').value,
        descriptionEn: document.getElementById('train-description-en').value,
        descriptionAr: document.getElementById('train-description-ar').value,
        routeStops
    };

    try {
        if (id) {
            await apiFetch(`/api/admin/trains/${id}`, {
                method: 'PUT',
                body: JSON.stringify(body)
            });
        } else {
            await apiFetch('/api/admin/trains', {
                method: 'POST',
                body: JSON.stringify(body)
            });
        }
        closeTrainModal();
        loadTrains();
    } catch (err) {
        alert(`Error saving train: ${err.message}`);
    }
});

async function deleteTrain(id) {
    if (!confirm(t('confirm_delete_train'))) return;
    try {
        await apiFetch(`/api/admin/trains/${id}`, { method: 'DELETE' });
        loadTrains();
    } catch (err) {
        alert(`Error deleting train: ${err.message}`);
    }
}

// Search Filter
document.getElementById('train-search').addEventListener('input', (e) => {
    const query = e.target.value.toLowerCase();
    const rows = document.querySelectorAll('#trains-table tbody tr');
    rows.forEach(row => {
        if (row.cells.length < 2) return;
        const text = row.innerText.toLowerCase();
        if (text.includes(query)) {
            row.style.display = '';
        } else {
            row.style.display = 'none';
        }
    });
});

// ==========================================
// 📅 TRIPS CRUD LOGIC
// ==========================================
async function loadTrips() {
    const tableBody = document.querySelector('#trips-table tbody');
    tableBody.innerHTML = `<tr><td colspan="7" class="loading-cell">${t('loading_trips')}</td></tr>`;
    
    try {
        const trips = await apiFetch('/api/admin/trips');
        tableBody.innerHTML = '';

        if (trips.length === 0) {
            tableBody.innerHTML = `<tr><td colspan="7" class="no-data-cell">${t('no_trips')}</td></tr>`;
            return;
        }

        trips.forEach(trip => {
            const tr = document.createElement('tr');
            
            // Format status badge
            const statusConfig = TRIP_STATUS_MAP[trip.status] || { text: trip.status, class: 'scheduled' };
            const statusText = t(`status_${statusConfig.class}`) || statusConfig.text;
            const statusBadge = `<span class="status-pill ${statusConfig.class}">${statusText}</span>`;
            
            // Format dates
            const formatDate = (dateStr) => dateStr ? new Date(dateStr).toLocaleString() : '-';

            const trainName = state.language === 'ar' ? (trip.trainNameAr || trip.trainNameEn) : (trip.trainNameEn || trip.trainNameAr);

            tr.innerHTML = `
                <td><strong>${trip.trainNumber} - ${trainName || ''}</strong></td>
                <td><code>${trip.tripDate}</code></td>
                <td>${statusBadge}</td>
                <td>${formatDate(trip.actualDeparture)}</td>
                <td>${formatDate(trip.actualArrival)}</td>
                <td>
                    <button class="btn btn-outline" style="padding: 4px 8px; font-size: 12px; height: auto;" onclick="openTripFollowersModal('${trip.id}', '${trip.trainNumber.replace(/'/g, "\\'")}', '${trip.tripDate}')">
                        <i class="fa-solid fa-users"></i> ${trip.followerCount || 0}
                    </button>
                </td>
                <td class="actions-column">
                    <button class="action-btn view" onclick="viewTrip('${trip.id}')" title="View"><i class="fa-solid fa-eye"></i></button>
                    <button class="action-btn edit" onclick="openTripStatusModal('${trip.id}', '${trip.status}', '${trip.actualDeparture || ''}', '${trip.actualArrival || ''}')" title="Change Status"><i class="fa-solid fa-rotate"></i></button>
                    <button class="action-btn delete" onclick="deleteTrip('${trip.id}')" title="Delete"><i class="fa-solid fa-trash"></i></button>
                </td>
            `;
            tableBody.appendChild(tr);
        });
    } catch (err) {
        tableBody.innerHTML = `<tr><td colspan="7" class="no-data-cell" style="color:var(--accent-red)">Error loading trips: ${err.message}</td></tr>`;
    }
}

// Modal Handlers
async function openTripModal() {
    const modal = document.getElementById('trip-modal');
    document.getElementById('trip-form').reset();
    
    // Set default date to today
    const today = new Date().toISOString().split('T')[0];
    document.getElementById('trip-date').value = today;

    // Load train options into dropdown
    const select = document.getElementById('trip-train-select');
    select.innerHTML = `<option value="">-- ${t('modal_trip_train')} --</option>`;

    try {
        if (state.trains.length === 0) {
            state.trains = await apiFetch('/api/admin/trains');
        }
        state.trains.forEach(train => {
            const trainName = state.language === 'ar' ? (train.nameAr || train.nameEn) : (train.nameEn || train.nameAr);
            select.innerHTML += `<option value="${train.id}">${train.trainNumber} - ${trainName}</option>`;
        });
    } catch (err) {
        console.error('Failed to load trains for dropdown', err);
    }

    modal.classList.remove('hidden');
    applyLocalization();
}

function closeTripModal() {
    document.getElementById('trip-modal').classList.add('hidden');
}

document.getElementById('trip-form').addEventListener('submit', async (e) => {
    e.preventDefault();
    const body = {
        trainId: document.getElementById('trip-train-select').value,
        tripDate: document.getElementById('trip-date').value,
        status: parseInt(document.getElementById('trip-status-select').value)
    };

    try {
        await apiFetch('/api/admin/trips', {
            method: 'POST',
            body: JSON.stringify(body)
        });
        closeTripModal();
        loadTrips();
    } catch (err) {
        alert(`Error scheduling trip: ${err.message}`);
    }
});

// Trip Status Update Modal handlers
function openTripStatusModal(tripId, status, departure, arrival) {
    const modal = document.getElementById('trip-status-modal');
    document.getElementById('status-trip-id').value = tripId;
    
    // Select status option in dropdown
    const select = document.getElementById('update-status-select');
    
    // If status is string, find numeric value
    let numStatus = 0;
    if (isNaN(status)) {
        const entry = Object.entries(TRIP_STATUS_MAP).find(([k, v]) => v.text === status || k === status);
        if (entry) {
            numStatus = isNaN(entry[0]) ? 0 : parseInt(entry[0]);
        }
    } else {
        numStatus = parseInt(status);
    }
    
    select.value = numStatus;

    // Format ISO dates to datetime-local values
    const formatDateTimeLocal = (isoStr) => {
        if (!isoStr) return '';
        const d = new Date(isoStr);
        const tzoffset = d.getTimezoneOffset() * 60000;
        const localISOTime = (new Date(d.getTime() - tzoffset)).toISOString().slice(0, 16);
        return localISOTime;
    };

    document.getElementById('update-actual-departure').value = formatDateTimeLocal(departure);
    document.getElementById('update-actual-arrival').value = formatDateTimeLocal(arrival);

    modal.classList.remove('hidden');
    applyLocalization();
}

function closeTripStatusModal() {
    document.getElementById('trip-status-modal').classList.add('hidden');
}

document.getElementById('trip-status-form').addEventListener('submit', async (e) => {
    e.preventDefault();
    const id = document.getElementById('status-trip-id').value;
    
    const depVal = document.getElementById('update-actual-departure').value;
    const arrVal = document.getElementById('update-actual-arrival').value;

    const body = {
        status: parseInt(document.getElementById('update-status-select').value),
        actualDeparture: depVal ? new Date(depVal).toISOString() : null,
        actualArrival: arrVal ? new Date(arrVal).toISOString() : null
    };

    try {
        await apiFetch(`/api/admin/trips/${id}/status`, {
            method: 'PUT',
            body: JSON.stringify(body)
        });
        closeTripStatusModal();
        loadTrips();
    } catch (err) {
        alert(`Error updating trip status: ${err.message}`);
    }
});

async function deleteTrip(id) {
    if (!confirm(t('confirm_delete_trip'))) return;
    try {
        await apiFetch(`/api/admin/trips/${id}`, { method: 'DELETE' });
        loadTrips();
    } catch (err) {
        alert(`Error deleting trip: ${err.message}`);
    }
}

// ==========================================
// 📩 SUGGESTIONS REVIEW LOGIC
// ==========================================
async function loadSuggestions() {
    const tableBody = document.querySelector('#suggestions-table tbody');
    tableBody.innerHTML = `<tr><td colspan="7" class="loading-cell">${t('loading_suggestions')}</td></tr>`;
    
    try {
        const suggestions = await apiFetch('/api/admin/suggestions');
        state.suggestions = suggestions; // Cache suggestions
        tableBody.innerHTML = '';

        if (suggestions.length === 0) {
            tableBody.innerHTML = `<tr><td colspan="7" class="no-data-cell">${t('no_suggestions')}</td></tr>`;
            return;
        }

        suggestions.forEach(s => {
            const tr = document.createElement('tr');
            const dateStr = new Date(s.createdAt).toLocaleDateString();

            const sugName = state.language === 'ar' ? (s.nameAr || s.nameEn) : (s.nameEn || s.nameAr);
            const sugDesc = state.language === 'ar' ? (s.descriptionAr || s.descriptionEn) : (s.descriptionEn || s.descriptionAr);
            const sugRoute = state.language === 'ar' ? (s.routeDescriptionAr || s.routeDescriptionEn) : (s.routeDescriptionEn || s.routeDescriptionAr);

            tr.innerHTML = `
                <td><strong>${s.trainNumber}</strong></td>
                <td style="font-weight: 600; color: white;">${sugName || ''}</td>
                <td style="color: var(--text-secondary); font-size: 13px;">${sugDesc || ''}</td>
                <td style="color: var(--text-secondary); font-size: 13px;">${sugRoute || ''}</td>
                <td><i class="fa-regular fa-user"></i> ${s.suggestedByName}</td>
                <td><code>${dateStr}</code></td>
                <td class="actions-column">
                    <button class="action-btn view" onclick="viewSuggestion('${s.id}')" title="View"><i class="fa-solid fa-eye"></i></button>
                    <button class="action-btn review" onclick="openSuggestionModal('${s.id}', '${s.trainNumber}', '${s.nameEn} / ${s.nameAr}', '${s.routeDescriptionEn || ''} / ${s.routeDescriptionAr || ''}', '${s.suggestedByName}')" title="Review"><i class="fa-solid fa-clipboard-check"></i> ${t('moderate_btn')}</button>
                </td>
            `;
            tableBody.appendChild(tr);
        });
    } catch (err) {
        tableBody.innerHTML = `<tr><td colspan="7" class="no-data-cell" style="color:var(--accent-red)">Error loading suggestions: ${err.message}</td></tr>`;
    }
}

function openSuggestionModal(id, number, name, route, author) {
    const modal = document.getElementById('suggestion-modal');
    document.getElementById('suggestion-form').reset();

    document.getElementById('review-suggestion-id').value = id;
    document.getElementById('review-train-info').textContent = `${number} - ${name}`;
    document.getElementById('review-route-info').textContent = route || 'None';
    document.getElementById('review-author-info').textContent = author;

    modal.classList.remove('hidden');
    applyLocalization();
}

function closeSuggestionModal() {
    document.getElementById('suggestion-modal').classList.add('hidden');
}

document.getElementById('suggestion-form').addEventListener('submit', async (e) => {
    e.preventDefault();
    const id = document.getElementById('review-suggestion-id').value;
    const body = {
        status: parseInt(document.getElementById('review-status-select').value),
        adminNotes: document.getElementById('review-notes').value
    };

    try {
        await apiFetch(`/api/admin/suggestions/${id}/review`, {
            method: 'PUT',
            body: JSON.stringify(body)
        });
        closeSuggestionModal();
        loadSuggestions();
        fetchSuggestionCountBadge();
    } catch (err) {
        alert(`Error reviewing suggestion: ${err.message}`);
    }
});

// ==========================================================================
// 👁️ DETAILS MODAL VIEW LOGIC
// ==========================================================================
function closeDetailsModal() {
    document.getElementById('details-modal').classList.add('hidden');
}

function showDetailsModal(title, contentHtml, isLarge = false) {
    const modal = document.getElementById('details-modal');
    const titleEl = document.getElementById('details-modal-title');
    const bodyEl = document.getElementById('details-modal-body');
    const cardEl = modal.querySelector('.modal-card');

    titleEl.textContent = title;
    bodyEl.innerHTML = contentHtml;

    if (isLarge) {
        cardEl.classList.add('modal-large');
    } else {
        cardEl.classList.remove('modal-large');
    }

    modal.classList.remove('hidden');
}

let stopViewMap = null;

function initStopViewMap(lat, lng) {
    const mapDiv = document.getElementById('stop-view-map');
    if (!mapDiv) return;

    if (typeof L === 'undefined') {
        console.warn('Leaflet is not loaded. View map is unavailable.');
        mapDiv.innerHTML = `
            <div style="display:flex; flex-direction:column; align-items:center; justify-content:center; height:100%; color:var(--text-secondary); font-size:13px; padding:20px; text-align:center; background:rgba(255,255,255,0.01);">
                <i class="fa-solid fa-triangle-exclamation" style="margin-bottom:8px; font-size:24px; color:var(--accent-orange);"></i>
                <span>Map is currently unavailable.</span>
            </div>
        `;
        return;
    }

    try {
        if (stopViewMap) {
            try {
                stopViewMap.remove();
            } catch (e) {
                console.error('Failed to remove previous view map:', e);
            }
            stopViewMap = null;
        }

        stopViewMap = L.map('stop-view-map').setView([lat, lng], 13);
        
        L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
            maxZoom: 19,
            attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
        }).addTo(stopViewMap);

        L.marker([lat, lng]).addTo(stopViewMap);

        setTimeout(() => {
            if (stopViewMap) stopViewMap.invalidateSize();
        }, 150);
    } catch (e) {
        console.error('Error loading Leaflet view map:', e);
    }
}

function viewStop(id) {
    const stop = state.stops.find(s => s.id === id);
    if (!stop) {
        alert('Stop details not found.');
        return;
    }

    const stopName = state.language === 'ar' ? (stop.nameAr || stop.nameEn) : (stop.nameEn || stop.nameAr);
    const stopCity = state.language === 'ar' ? (stop.cityAr || stop.cityEn) : (stop.cityEn || stop.cityAr);
    const stopDesc = state.language === 'ar' ? (stop.descriptionAr || stop.descriptionEn) : (stop.descriptionEn || stop.descriptionAr);

    const alignmentStyle = state.language === 'ar' ? 'direction: rtl; text-align: right;' : 'direction: ltr; text-align: left;';

    const html = `
        <div style="display: flex; flex-direction: column; gap: 16px; ${alignmentStyle}">
            <div style="display: flex; align-items: center; gap: 12px; margin-bottom: 8px;">
                <span style="font-size: 24px; color: var(--accent-green);"><i class="fa-solid fa-map-location-dot"></i></span>
                <div>
                    <h3 style="font-size: 18px; margin: 0; color: white;">${stopName}</h3>
                </div>
            </div>
            
            <div style="background: rgba(255,255,255,0.02); border-radius: 8px; border: 1px solid var(--border-color); overflow: hidden;">
                <table style="width: 100%; border-collapse: collapse; font-size: 13px;">
                    <tbody>
                        <tr>
                            <td style="padding: 10px 16px; border-bottom: 1px solid var(--border-color); color: var(--text-secondary); font-weight: 500; width: 140px;">${t('table_col_code')}</td>
                            <td style="padding: 10px 16px; border-bottom: 1px solid var(--border-color); color: white;"><code>${stop.code}</code></td>
                        </tr>
                        <tr>
                            <td style="padding: 10px 16px; border-bottom: 1px solid var(--border-color); color: var(--text-secondary); font-weight: 500;">${t('table_col_city')}</td>
                            <td style="padding: 10px 16px; border-bottom: 1px solid var(--border-color); color: white;">${stopCity || '-'}</td>
                        </tr>
                        <tr>
                            <td style="padding: 10px 16px; border-bottom: 1px solid var(--border-color); color: var(--text-secondary); font-weight: 500;">${t('table_col_lat')}</td>
                            <td style="padding: 10px 16px; border-bottom: 1px solid var(--border-color); color: white;"><code>${stop.latitude.toFixed(6)}</code></td>
                        </tr>
                        <tr>
                            <td style="padding: 10px 16px; border-bottom: 1px solid var(--border-color); color: var(--text-secondary); font-weight: 500;">${t('table_col_lng')}</td>
                            <td style="padding: 10px 16px; border-bottom: 1px solid var(--border-color); color: white;"><code>${stop.longitude.toFixed(6)}</code></td>
                        </tr>
                        <tr>
                            <td style="padding: 10px 16px; border-bottom: 1px solid var(--border-color); color: var(--text-secondary); font-weight: 500;">${t('table_col_description')}</td>
                            <td style="padding: 10px 16px; border-bottom: 1px solid var(--border-color); color: white;">${stopDesc || '-'}</td>
                        </tr>
                    </tbody>
                </table>
            </div>

            <div>
                <span style="display: block; font-size: 11px; text-transform: uppercase; color: var(--text-secondary); margin-bottom: 8px;"><i class="fa-solid fa-map-location-dot"></i> Map Location</span>
                <div id="stop-view-map" style="height: 200px; border-radius: 8px; border: 1px solid var(--border-color); overflow: hidden; position: relative;"></div>
            </div>
        </div>
    `;

    showDetailsModal(t('modal_details_title'), html, false);
    initStopViewMap(stop.latitude, stop.longitude);
}

let currentDetailsTrainId = null;

async function viewTrain(id) {
    const train = state.trains.find(t => t.id === id);
    if (!train) {
        alert('Train details not found.');
        return;
    }
    
    currentDetailsTrainId = id;

    const trainName = state.language === 'ar' ? (train.nameAr || train.nameEn) : (train.nameEn || train.nameAr);
    const trainDesc = state.language === 'ar' ? (train.descriptionAr || train.descriptionEn) : (train.descriptionEn || train.descriptionAr);
    const activeText = train.isActive ? t('status_active') : t('status_inactive');
    const activeClass = train.isActive ? 'arrived' : 'cancelled';

    const alignmentStyle = state.language === 'ar' ? 'direction: rtl; text-align: right;' : 'direction: ltr; text-align: left;';

    const html = `
        <div style="display: flex; flex-direction: column; gap: 16px; ${alignmentStyle}">
            <div style="display: flex; align-items: center; justify-content: space-between; margin-bottom: 8px;">
                <div style="display: flex; align-items: center; gap: 12px;">
                    <span style="font-size: 24px; color: var(--accent-purple);"><i class="fa-solid fa-train"></i></span>
                    <div>
                        <h3 style="font-size: 18px; margin: 0; color: white;">${t('table_col_train')} ${train.trainNumber}</h3>
                        <p style="margin: 2px 0 0 0; color: var(--text-secondary); font-size: 14px;">${trainName}</p>
                    </div>
                </div>
                <span class="status-pill ${activeClass}" style="font-size: 11px; font-weight: 700;">
                    ${activeText}
                </span>
            </div>
            
            <div style="background: rgba(255,255,255,0.02); border-radius: 8px; border: 1px solid var(--border-color); overflow: hidden;">
                <table style="width: 100%; border-collapse: collapse; font-size: 13px;">
                    <tbody>
                        <tr>
                            <td style="padding: 10px 16px; border-bottom: 1px solid var(--border-color); color: var(--text-secondary); font-weight: 500; width: 140px;">${t('table_col_train_no')}</td>
                            <td style="padding: 10px 16px; border-bottom: 1px solid var(--border-color); color: white;"><strong>${train.trainNumber}</strong></td>
                        </tr>
                        <tr>
                            <td style="padding: 10px 16px; border-bottom: 1px solid var(--border-color); color: var(--text-secondary); font-weight: 500;">${t('table_col_train_name')}</td>
                            <td style="padding: 10px 16px; border-bottom: 1px solid var(--border-color); color: white;">${trainName}</td>
                        </tr>
                        <tr>
                            <td style="padding: 10px 16px; color: var(--text-secondary); font-weight: 500;">${t('table_col_description')}</td>
                            <td style="padding: 10px 16px; color: white;">${trainDesc || '-'}</td>
                        </tr>
                    </tbody>
                </table>
            </div>
            
            <div>
                <h4 style="font-size: 13px; text-transform: uppercase; color: var(--text-secondary); margin-bottom: 10px; font-weight: 600; display: flex; align-items: center; gap: 6px;">
                    <i class="fa-solid fa-route"></i> ${t('modal_train_route_stops')}
                </h4>
                <div style="background: rgba(255,255,255,0.02); border-radius: 8px; border: 1px solid var(--border-color); overflow: hidden;">
                    <table style="width: 100%; border-collapse: collapse; font-size: 13px;">
                        <thead>
                            <tr style="background: rgba(255,255,255,0.02);">
                                <th style="padding: 10px 16px; color: var(--text-secondary); font-size: 11px; text-transform: uppercase;">${t('modal_train_seq')}</th>
                                <th style="padding: 10px 16px; color: var(--text-secondary); font-size: 11px; text-transform: uppercase;">${t('modal_train_station')}</th>
                                <th style="padding: 10px 16px; color: var(--text-secondary); font-size: 11px; text-transform: uppercase;">${t('modal_train_arr')}</th>
                                <th style="padding: 10px 16px; color: var(--text-secondary); font-size: 11px; text-transform: uppercase;">${t('modal_train_dep')}</th>
                            </tr>
                        </thead>
                        <tbody>
                            ${train.routeStops.sort((a, b) => a.stopOrder - b.stopOrder).map(rs => {
                                const stopName = state.language === 'ar' ? (rs.stopNameAr || rs.stopNameEn) : (rs.stopNameEn || rs.stopNameAr);
                                return `
                                    <tr>
                                        <td style="padding: 10px 16px; border-bottom: 1px solid var(--border-color);">
                                            <span style="background: rgba(168, 85, 247, 0.1); color: var(--accent-purple); width: 22px; height: 22px; display: inline-flex; align-items: center; justify-content: center; border-radius: 50%; font-size: 11px; font-weight: 700;">
                                                ${rs.stopOrder}
                                            </span>
                                        </td>
                                        <td style="padding: 10px 16px; border-bottom: 1px solid var(--border-color); color: white; font-weight: 500;">
                                            ${stopName} <span style="color: var(--text-muted); font-size: 11px;">(${rs.stopCode})</span>
                                        </td>
                                        <td style="padding: 10px 16px; border-bottom: 1px solid var(--border-color); color: var(--text-secondary);">
                                            ${rs.scheduledArrival ? rs.scheduledArrival.substring(0, 5) : '--:--'}
                                        </td>
                                        <td style="padding: 10px 16px; border-bottom: 1px solid var(--border-color); color: var(--text-secondary);">
                                            ${rs.scheduledDeparture ? rs.scheduledDeparture.substring(0, 5) : '--:--'}
                                        </td>
                                    </tr>
                                `;
                            }).join('')}
                        </tbody>
                    </table>
                </div>
            </div>
        </div>
    `;

    document.getElementById('train-details-info-container').innerHTML = html;
    switchTab('train-details');
    
    // Initialize states
    detailsState.trainTrips = [];
    detailsState.trainTripsPage = 1;
    detailsState.trainTripsDateFrom = '';
    detailsState.trainTripsDateTo = '';
    detailsState.trainFollowersSearch = '';
    detailsState.trainFollowersPage = 1;
    
    await loadTrainDetailsTripsFromApi(id);
    await loadTrainDetailsFollowers();
}

async function loadTrainDetailsTripsFromApi(trainId) {
    const card = document.getElementById('train-details-trips-card');
    if (!card) return;

    const isRtl = state.language === 'ar';
    card.innerHTML = `
        <div style="text-align: center; padding: 30px; color: var(--text-muted);">
            <i class="fa-solid fa-circle-notch fa-spin"></i> ${isRtl ? 'جاري التحميل...' : 'Loading trips...'}
        </div>
    `;

    try {
        const trips = await apiFetch(`/api/trains/${trainId}/trips`);
        detailsState.trainTrips = trips || [];
        loadTrainDetailsTrips();
    } catch (err) {
        card.innerHTML = `
            <div style="color: var(--accent-red); font-size: 13px; text-align: center; padding: 20px;">
                Error: ${err.message}
            </div>
        `;
    }
}

function loadTrainDetailsTrips() {
    const card = document.getElementById('train-details-trips-card');
    if (!card) return;

    const dateFrom = detailsState.trainTripsDateFrom;
    const dateTo = detailsState.trainTripsDateTo;

    const filtered = detailsState.trainTrips.filter(t => {
        if (dateFrom && t.tripDate < dateFrom) return false;
        if (dateTo && t.tripDate > dateTo) return false;
        return true;
    });

    const pageSize = 5;
    const totalPages = Math.ceil(filtered.length / pageSize) || 1;
    if (detailsState.trainTripsPage > totalPages) detailsState.trainTripsPage = totalPages;
    const pageIndex = detailsState.trainTripsPage - 1;
    const paginated = filtered.slice(pageIndex * pageSize, (pageIndex + 1) * pageSize);

    const alignmentStyle = state.language === 'ar' ? 'direction: rtl; text-align: right;' : 'direction: ltr; text-align: left;';
    const isRtl = state.language === 'ar';

    let html = `
        <div style="${alignmentStyle}">
            <h3 style="font-size: 15px; font-weight: 700; color: white; display: flex; align-items: center; gap: 8px; margin: 0 0 16px 0;">
                <i class="fa-solid fa-calendar-days" style="color: var(--accent-purple);"></i>
                <span>${isRtl ? 'الرحلات الأخيرة' : 'Latest Trips'}</span>
            </h3>
            
            <div style="display: grid; grid-template-columns: 1fr 1fr; gap: 12px; margin-bottom: 16px;">
                <div>
                    <label style="font-size: 11px; color: var(--text-secondary); display: block; margin-bottom: 4px;">
                        ${isRtl ? 'من تاريخ' : 'Date From'}
                    </label>
                    <input type="date" class="form-control" id="train-trips-filter-from" value="${dateFrom}" 
                        style="width: 100%; height: 36px; padding: 6px 12px; font-size: 13px; background: rgba(0,0,0,0.2); border: 1px solid var(--border-color); border-radius: 6px; color: white;" 
                        onchange="setTrainTripsFilter('from', this.value)" />
                </div>
                <div>
                    <label style="font-size: 11px; color: var(--text-secondary); display: block; margin-bottom: 4px;">
                        ${isRtl ? 'إلى تاريخ' : 'Date To'}
                    </label>
                    <input type="date" class="form-control" id="train-trips-filter-to" value="${dateTo}" 
                        style="width: 100%; height: 36px; padding: 6px 12px; font-size: 13px; background: rgba(0,0,0,0.2); border: 1px solid var(--border-color); border-radius: 6px; color: white;" 
                        onchange="setTrainTripsFilter('to', this.value)" />
                </div>
            </div>

            <div style="display: flex; flex-direction: column; gap: 10px; min-height: 100px;">
    `;

    if (paginated.length === 0) {
        html += `
            <div style="color: var(--text-muted); font-size: 13px; text-align: center; padding: 30px 10px;">
                ${isRtl ? 'لا توجد رحلات مجدولة.' : 'No trips scheduled.'}
            </div>
        `;
    } else {
        paginated.forEach(trip => {
            const statusConfig = TRIP_STATUS_MAP[trip.status] || { text: trip.status, class: 'scheduled' };
            const statusText = t(`status_${statusConfig.class}`) || statusConfig.text;
            html += `
                <div onclick="viewTrip('${trip.id}')"
                    style="display: flex; align-items: center; justify-content: space-between; padding: 10px 14px; border-radius: 8px; border: 1px solid var(--border-color); background: rgba(255,255,255,0.015); cursor: pointer; transition: all 0.2s;"
                    onmouseover="this.style.background='rgba(255,255,255,0.03)'"
                    onmouseout="this.style.background='rgba(255,255,255,0.015)'">
                    <div>
                        <div style="font-weight: 600; color: white; font-size: 13px;">${trip.tripDate}</div>
                        <div style="font-size: 11px; color: var(--text-secondary); margin-top: 2px;">
                            ${isRtl ? `المتابعون: ${trip.followerCount}` : `Followers: ${trip.followerCount}`}
                        </div>
                    </div>
                    <div style="display: flex; align-items: center; gap: 8px;">
                        <span class="status-pill ${statusConfig.class}" style="font-size: 10px; padding: 2px 6px;">
                            ${statusText}
                        </span>
                        <i class="fa-solid fa-arrow-right" style="font-size: 12px; color: var(--text-muted); transform: ${isRtl ? 'rotate(180deg)' : 'none'}"></i>
                    </div>
                </div>
            `;
        });
    }

    html += `</div>`;

    if (totalPages > 1) {
        html += `
            <div style="display: flex; justify-content: center; align-items: center; gap: 12px; margin-top: 14px;">
                <button class="btn btn-secondary" onclick="changeTrainTripsPage(-1)" ${detailsState.trainTripsPage === 1 ? 'disabled' : ''} style="padding: 4px 10px; font-size: 11px; height: 26px; min-width: auto; margin: 0;">
                    ${isRtl ? 'السابق' : 'Prev'}
                </button>
                <span style="font-size: 11px; color: var(--text-secondary);">
                    ${isRtl ? `صفحة ${detailsState.trainTripsPage} من ${totalPages}` : `Page ${detailsState.trainTripsPage} of ${totalPages}`}
                </span>
                <button class="btn btn-secondary" onclick="changeTrainTripsPage(1)" ${detailsState.trainTripsPage === totalPages ? 'disabled' : ''} style="padding: 4px 10px; font-size: 11px; height: 26px; min-width: auto; margin: 0;">
                    ${isRtl ? 'التالي' : 'Next'}
                </button>
            </div>
        `;
    }

    html += `</div>`;
    card.innerHTML = html;
}

function setTrainTripsFilter(type, value) {
    if (type === 'from') detailsState.trainTripsDateFrom = value;
    if (type === 'to') detailsState.trainTripsDateTo = value;
    detailsState.trainTripsPage = 1;
    loadTrainDetailsTrips();
}

function changeTrainTripsPage(dir) {
    detailsState.trainTripsPage += dir;
    loadTrainDetailsTrips();
}

async function loadTrainDetailsFollowers() {
    if (!currentDetailsTrainId) return;
    const card = document.getElementById('train-details-followers-card');
    if (!card) return;

    const isRtl = state.language === 'ar';
    const alignmentStyle = state.language === 'ar' ? 'direction: rtl; text-align: right;' : 'direction: ltr; text-align: left;';

    card.innerHTML = `
        <div style="${alignmentStyle}">
            <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 16px;">
                <h3 style="font-size: 15px; font-weight: 700; color: white; display: flex; align-items: center; gap: 8px; margin: 0;">
                    <i class="fa-solid fa-users" style="color: var(--accent-purple);"></i>
                    <span>${isRtl ? 'المتابعون' : 'Followers'} (<span id="train-followers-count">0</span>)</span>
                </h3>
                <button class="btn btn-secondary" id="btn-details-remove-all-train-followers" onclick="detailsRemoveAllTrainFollowers()" 
                    style="background: rgba(239, 68, 68, 0.15); color: #fca5a5; border-color: rgba(239, 68, 68, 0.3); font-size: 11px; padding: 4px 8px; height: 26px; margin: 0; display: none;">
                    <i class="fa-solid fa-trash-can"></i> ${isRtl ? 'حذف الكل' : 'Clear All'}
                </button>
            </div>
            
            <div style="position: relative; margin-bottom: 16px;">
                <i class="fa-solid fa-magnifying-glass" style="position: absolute; ${isRtl ? 'right' : 'left'}: 12px; top: 50%; transform: translateY(-50%); color: var(--text-muted); font-size: 13px;"></i>
                <input type="text" class="form-control" id="train-followers-search" 
                    placeholder="${isRtl ? 'البحث بالاسم أو البريد...' : 'Search by name or email...'}"
                    value="${detailsState.trainFollowersSearch}"
                    oninput="setTrainFollowersSearch(this.value)"
                    style="${isRtl ? 'padding: 6px 12px 6px 36px;' : 'padding: 6px 36px 6px 12px;'} width: 100%; height: 36px; font-size: 13px; background: rgba(0,0,0,0.2); border: 1px solid var(--border-color); border-radius: 6px; color: white;" />
            </div>

            <div id="train-followers-list-container" style="display: flex; flex-direction: column; gap: 10px; min-height: 100px;">
                <div style="text-align: center; padding: 30px; color: var(--text-muted);">
                    <i class="fa-solid fa-circle-notch fa-spin"></i> ${isRtl ? 'جاري التحميل...' : 'Loading followers...'}
                </div>
            </div>
            
            <div id="train-followers-pagination" style="display: flex; justify-content: center; align-items: center; gap: 12px; margin-top: 14px;"></div>
        </div>
    `;

    try {
        const followers = await apiFetch(`/api/admin/trains/${currentDetailsTrainId}/followers`);
        detailsState.trainFollowers = followers || [];
        renderTrainFollowersList();
    } catch (err) {
        document.getElementById('train-followers-list-container').innerHTML = `
            <div style="color: var(--accent-red); font-size: 13px; text-align: center; padding: 20px;">
                Error: ${err.message}
            </div>
        `;
    }
}

function setTrainFollowersSearch(value) {
    detailsState.trainFollowersSearch = value;
    detailsState.trainFollowersPage = 1;
    renderTrainFollowersList();
}

function renderTrainFollowersList() {
    const listContainer = document.getElementById('train-followers-list-container');
    const paginationContainer = document.getElementById('train-followers-pagination');
    const countSpan = document.getElementById('train-followers-count');
    const clearBtn = document.getElementById('btn-details-remove-all-train-followers');
    if (!listContainer) return;

    const isRtl = state.language === 'ar';
    const query = detailsState.trainFollowersSearch.toLowerCase();

    const filtered = detailsState.trainFollowers.filter(f => 
        (f.displayName && f.displayName.toLowerCase().includes(query)) ||
        (f.email && f.email.toLowerCase().includes(query))
    );

    countSpan.textContent = filtered.length;

    if (filtered.length > 0) {
        clearBtn.style.display = 'inline-block';
    } else {
        clearBtn.style.display = 'none';
    }

    const pageSize = 5;
    const totalPages = Math.ceil(filtered.length / pageSize) || 1;
    if (detailsState.trainFollowersPage > totalPages) detailsState.trainFollowersPage = totalPages;
    const pageIndex = detailsState.trainFollowersPage - 1;
    const paginated = filtered.slice(pageIndex * pageSize, (pageIndex + 1) * pageSize);

    let html = '';
    if (paginated.length === 0) {
        html = `
            <div style="color: var(--text-muted); font-size: 13px; text-align: center; padding: 30px 10px;">
                ${isRtl ? 'لا يوجد متابعون.' : 'No followers found.'}
            </div>
        `;
    } else {
        paginated.forEach(follower => {
            const initial = follower.displayName ? follower.displayName[0].toUpperCase() : 'U';
            
            let daysHtml = '';
            if (follower.daysOfWeek && follower.daysOfWeek.length > 0) {
                const dayLabelsEn = ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'];
                const dayLabelsAr = ['ح', 'ن', 'ث', 'ر', 'خ', 'ج', 'س'];
                daysHtml = '<div style="display: flex; gap: 4px; flex-wrap: wrap; justify-content: flex-end; max-width: 120px;">';
                follower.daysOfWeek.forEach(day => {
                    const label = isRtl ? dayLabelsAr[day] : dayLabelsEn[day];
                    daysHtml += `<span class="status-pill active" style="font-size: 9px; padding: 2px 4px; font-weight: 500;" title="${dayLabelsEn[day]}">${label}</span>`;
                });
                daysHtml += '</div>';
            }

            html += `
                <div style="display: flex; align-items: center; justify-content: space-between; padding: 10px 14px; border-radius: 8px; border: 1px solid var(--border-color); background: rgba(255,255,255,0.015); gap: 12px;">
                    <div style="display: flex; align-items: center; gap: 10px; min-width: 0;">
                        <div style="width: 32px; height: 32px; border-radius: 50%; background: var(--accent-purple); color: white; display: flex; align-items: center; justify-content: center; font-weight: 700; font-size: 13px; flex-shrink: 0;">
                            ${initial}
                        </div>
                        <div style="min-width: 0;">
                            <div style="font-weight: 600; color: white; font-size: 13px; overflow: hidden; text-overflow: ellipsis; white-space: nowrap;">${follower.displayName}</div>
                            <div style="font-size: 11px; color: var(--text-secondary); overflow: hidden; text-overflow: ellipsis; white-space: nowrap;">${follower.email}</div>
                        </div>
                    </div>
                    <div style="display: flex; align-items: center; gap: 12px; flex-shrink: 0;">
                        ${daysHtml}
                        <button class="action-btn delete" onclick="detailsRemoveTrainFollower('${follower.userId}')" title="${isRtl ? 'إزالة' : 'Remove'}" style="padding: 6px; height: auto;">
                            <i class="fa-solid fa-trash-can"></i>
                        </button>
                    </div>
                </div>
            `;
        });
    }

    listContainer.innerHTML = html;

    if (totalPages > 1) {
        paginationContainer.innerHTML = `
            <button class="btn btn-secondary" onclick="changeTrainFollowersPage(-1)" ${detailsState.trainFollowersPage === 1 ? 'disabled' : ''} style="padding: 4px 10px; font-size: 11px; height: 26px; min-width: auto; margin: 0;">
                ${isRtl ? 'السابق' : 'Prev'}
            </button>
            <span style="font-size: 11px; color: var(--text-secondary);">
                ${isRtl ? `صفحة ${detailsState.trainFollowersPage} من ${totalPages}` : `Page ${detailsState.trainFollowersPage} of ${totalPages}`}
            </span>
            <button class="btn btn-secondary" onclick="changeTrainFollowersPage(1)" ${detailsState.trainFollowersPage === totalPages ? 'disabled' : ''} style="padding: 4px 10px; font-size: 11px; height: 26px; min-width: auto; margin: 0;">
                ${isRtl ? 'التالي' : 'Next'}
            </button>
        `;
    } else {
        paginationContainer.innerHTML = '';
    }
}

function changeTrainFollowersPage(dir) {
    detailsState.trainFollowersPage += dir;
    renderTrainFollowersList();
}

async function detailsRemoveTrainFollower(userId) {
    const confirmMsg = state.language === 'ar'
        ? 'هل أنت متأكد من إزالة هذا المتابع؟'
        : 'Are you sure you want to remove this follower?';
    if (!confirm(confirmMsg)) return;
    
    try {
        await apiFetch(`/api/admin/trains/${currentDetailsTrainId}/followers/${userId}`, { method: 'DELETE' });
        await loadTrainDetailsFollowers();
        if (state.activeTab === 'trains') loadTrains();
    } catch (err) {
        alert(`Error: ${err.message}`);
    }
}

async function detailsRemoveAllTrainFollowers() {
    const confirmMsg = state.language === 'ar'
        ? 'هل أنت متأكد من إزالة جميع المتابعين لهذا القطار؟'
        : 'Are you sure you want to remove ALL followers for this train?';
    if (!confirm(confirmMsg)) return;
    
    try {
        await apiFetch(`/api/admin/trains/${currentDetailsTrainId}/followers`, { method: 'DELETE' });
        await loadTrainDetailsFollowers();
        if (state.activeTab === 'trains') loadTrains();
    } catch (err) {
        alert(`Error: ${err.message}`);
    }
}

let currentDetailsTripId = null;

async function viewTrip(id) {
    const infoContainer = document.getElementById('trip-details-info-container');
    infoContainer.innerHTML = `<p class="loading-text">${t('loading_trips')}</p>`;
    switchTab('trip-details');

    try {
        const trip = await apiFetch(`/api/trips/${id}`);
        if (!trip) throw new Error('Trip not found');

        currentDetailsTripId = id;

        const formatDate = (dateStr) => dateStr ? new Date(dateStr).toLocaleString() : t('status_inactive');
        const statusConfig = TRIP_STATUS_MAP[trip.status] || { text: trip.status, class: 'scheduled' };
        const statusText = t(`status_${statusConfig.class}`) || statusConfig.text;

        let updatesHtml = `<p style="color: var(--text-muted); font-size: 13px; margin: 10px 0;">${t('no_updates')}</p>`;
        if (trip.recentUpdates && trip.recentUpdates.length > 0) {
            updatesHtml = '<div style="display: flex; flex-direction: column; gap: 10px; margin-top: 10px;">';
            trip.recentUpdates.forEach(update => {
                const dateStr = new Date(update.createdAt).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
                let tagHtml = '';
                if (update.statusTag) {
                    const tagText = getLookupName('StatusTag', update.statusTag);
                    tagHtml = `<span class="status-pill ${update.statusTag.toLowerCase()}" style="font-size: 9px; padding: 2px 6px;">${tagText}</span>`;
                }
                let crowdHtml = '';
                if (update.crowdState) {
                    const crowdText = getLookupName('CrowdLevel', update.crowdState);
                    crowdHtml = `<span class="status-pill crowd-${update.crowdState.toLowerCase()}" style="font-size: 9px; padding: 2px 6px;">${crowdText}</span>`;
                }
                const updateBorder = state.language === 'ar' ? 'border-right: 3px solid var(--accent-purple); border-left: 1px solid var(--border-color);' : 'border-left: 3px solid var(--accent-purple); border-right: 1px solid var(--border-color);';
                updatesHtml += `
                    <div style="background: rgba(255,255,255,0.015); ${updateBorder} padding: 12px; border-radius: 6px; border-top: 1px solid var(--border-color); border-bottom: 1px solid var(--border-color);">
                        <div style="display: flex; justify-content: space-between; font-size: 11px; margin-bottom: 4px;">
                            <span style="color: white; font-weight: 600;">${update.authorName}</span>
                            <span style="color: var(--text-muted);">${dateStr}</span>
                        </div>
                        <p style="margin: 0; color: var(--text-secondary); font-size: 13px;">${update.content}</p>
                        ${update.statusTag || update.crowdState || update.latitude ? `
                        <div style="display: flex; gap: 8px; align-items: center; margin-top: 6px; font-size: 11px; color: var(--text-muted);">
                            ${tagHtml}
                            ${crowdHtml}
                            ${update.latitude ? `<span><i class="fa-solid fa-location-dot"></i> GPS: ${update.latitude.toFixed(4)}, ${update.longitude.toFixed(4)}</span>` : ''}
                        </div>
                        ` : ''}
                    </div>
                `;
            });
            updatesHtml += '</div>';
        }

        const trainName = state.language === 'ar' ? (trip.trainNameAr || trip.trainNameEn) : (trip.trainNameEn || trip.trainNameAr);
        
        const alignmentStyle = state.language === 'ar' ? 'direction: rtl; text-align: right;' : 'direction: ltr; text-align: left;';

        const html = `
            <div style="display: flex; flex-direction: column; gap: 18px; ${alignmentStyle}">
                <div style="display: flex; align-items: center; justify-content: space-between; border-bottom: 1px solid var(--border-color); padding-bottom: 12px;">
                    <div style="display: flex; align-items: center; gap: 12px;">
                        <span style="font-size: 24px; color: var(--accent-blue);"><i class="fa-solid fa-calendar-day"></i></span>
                        <div>
                            <h3 style="font-size: 18px; margin: 0; color: white;">${trip.trainNumber} - ${trainName}</h3>
                            <p style="margin: 6px 0 0 0; color: var(--text-secondary); font-size: 14px;">${t('modal_trip_date')}: <strong>${trip.tripDate}</strong></p>
                        </div>
                    </div>
                    <div style="display: flex; flex-direction: column; align-items: flex-end; gap: 6px;">
                        <span class="status-pill ${statusConfig.class}" style="font-size: 11px; font-weight: 700;">
                            ${statusText}
                        </span>
                    </div>
                </div>

                <div style="display: grid; grid-template-columns: 1fr 1fr; gap: 16px; background: rgba(255,255,255,0.02); padding: 16px; border-radius: 8px; border: 1px solid var(--border-color);">
                    <div>
                        <span style="display: block; font-size: 11px; text-transform: uppercase; color: var(--text-secondary); margin-bottom: 4px;"><i class="fa-solid fa-plane-departure"></i> ${t('table_col_actual_dep')}</span>
                        <strong style="color: white; font-size: 13px;">${formatDate(trip.actualDeparture)}</strong>
                    </div>
                    <div>
                        <span style="display: block; font-size: 11px; text-transform: uppercase; color: var(--text-secondary); margin-bottom: 4px;"><i class="fa-solid fa-plane-arrival"></i> ${t('table_col_actual_arr')}</span>
                        <strong style="color: white; font-size: 13px;">${formatDate(trip.actualArrival)}</strong>
                    </div>
                </div>

                <div style="display: grid; grid-template-columns: 1fr; gap: 20px;">
                    <div>
                        <h4 style="font-size: 12px; text-transform: uppercase; color: var(--text-secondary); margin-bottom: 8px; font-weight: 600; display: flex; align-items: center; gap: 6px;">
                            <i class="fa-solid fa-route"></i> ${t('modal_train_route_stops')}
                        </h4>
                        <div style="background: rgba(255,255,255,0.02); border-radius: 8px; border: 1px solid var(--border-color); overflow: hidden;">
                            <table style="width: 100%; border-collapse: collapse; font-size: 12px;">
                                <thead>
                                    <tr style="background: rgba(255,255,255,0.02);">
                                        <th style="padding: 8px 12px; color: var(--text-secondary); font-size: 10px; text-transform: uppercase;">${t('modal_train_station')}</th>
                                        <th style="padding: 8px 12px; color: var(--text-secondary); font-size: 10px; text-transform: uppercase;">${t('modal_train_arr')}</th>
                                        <th style="padding: 8px 12px; color: var(--text-secondary); font-size: 10px; text-transform: uppercase;">${t('modal_train_dep')}</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    ${trip.routeStops.sort((a, b) => a.stopOrder - b.stopOrder).map(rs => {
                                        const stopName = state.language === 'ar' ? (rs.stopNameAr || rs.stopNameEn) : (rs.stopNameEn || rs.stopNameAr);
                                        return `
                                            <tr>
                                                <td style="padding: 8px 12px; border-bottom: 1px solid var(--border-color); color: white; font-weight: 500;">
                                                    ${rs.stopOrder}. ${stopName}
                                                </td>
                                                <td style="padding: 8px 12px; border-bottom: 1px solid var(--border-color); color: var(--text-secondary);">
                                                    ${rs.scheduledArrival ? rs.scheduledArrival.substring(0, 5) : '--:--'}
                                                </td>
                                                <td style="padding: 8px 12px; border-bottom: 1px solid var(--border-color); color: var(--text-secondary);">
                                                    ${rs.scheduledDeparture ? rs.scheduledDeparture.substring(0, 5) : '--:--'}
                                                </td>
                                            </tr>
                                        `;
                                    }).join('')}
                                </tbody>
                            </table>
                        </div>
                    </div>

                    <div>
                        <h4 style="font-size: 12px; text-transform: uppercase; color: var(--text-secondary); margin-bottom: 8px; font-weight: 600; display: flex; align-items: center; gap: 6px;">
                            <i class="fa-solid fa-bell"></i> ${t('recent_crowd_updates')}
                        </h4>
                        <div style="max-height: 240px; overflow-y: auto; padding-right: 4px;">
                            ${updatesHtml}
                        </div>
                    </div>
                </div>
            </div>
        `;
        
        infoContainer.innerHTML = html;
        
        // Initialize state variables for trip-specific details
        detailsState.tripFollowers = [];
        detailsState.tripFollowersPage = 1;
        detailsState.tripFollowersSearch = '';

        await loadTripDetailsFollowers();
    } catch (err) {
        infoContainer.innerHTML = `<p style="color: var(--accent-red); text-align: center; padding: 20px;">Failed to load trip details: ${err.message}</p>`;
    }
}



async function loadTripDetailsFollowers() {
    if (!currentDetailsTripId) return;
    const card = document.getElementById('trip-details-followers-card');
    if (!card) return;

    const isRtl = state.language === 'ar';
    const alignmentStyle = state.language === 'ar' ? 'direction: rtl; text-align: right;' : 'direction: ltr; text-align: left;';

    card.innerHTML = `
        <div style="${alignmentStyle}">
            <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 16px;">
                <h3 style="font-size: 15px; font-weight: 700; color: white; display: flex; align-items: center; gap: 8px; margin: 0;">
                    <i class="fa-solid fa-users" style="color: var(--accent-orange);"></i>
                    <span>${isRtl ? 'المتابعون' : 'Followers'} (<span id="trip-followers-count">0</span>)</span>
                </h3>
                <button class="btn btn-secondary" id="btn-details-remove-all-trip-followers" onclick="detailsRemoveAllTripFollowers()" 
                    style="background: rgba(239, 68, 68, 0.15); color: #fca5a5; border-color: rgba(239, 68, 68, 0.3); font-size: 11px; padding: 4px 8px; height: 26px; margin: 0; display: none;">
                    <i class="fa-solid fa-trash-can"></i> ${isRtl ? 'حذف الكل' : 'Clear All'}
                </button>
            </div>
            
            <div style="position: relative; margin-bottom: 16px;">
                <i class="fa-solid fa-magnifying-glass" style="position: absolute; ${isRtl ? 'right' : 'left'}: 12px; top: 50%; transform: translateY(-50%); color: var(--text-muted); font-size: 13px;"></i>
                <input type="text" class="form-control" id="trip-followers-search" 
                    placeholder="${isRtl ? 'البحث بالاسم أو البريد...' : 'Search by name or email...'}"
                    value="${detailsState.tripFollowersSearch}"
                    oninput="setTripFollowersSearch(this.value)"
                    style="${isRtl ? 'padding: 6px 12px 6px 36px;' : 'padding: 6px 36px 6px 12px;'} width: 100%; height: 36px; font-size: 13px; background: rgba(0,0,0,0.2); border: 1px solid var(--border-color); border-radius: 6px; color: white;" />
            </div>

            <div id="trip-followers-list-container" style="display: flex; flex-direction: column; gap: 10px; min-height: 100px;">
                <div style="text-align: center; padding: 30px; color: var(--text-muted);">
                    <i class="fa-solid fa-circle-notch fa-spin"></i> ${isRtl ? 'جاري التحميل...' : 'Loading followers...'}
                </div>
            </div>
            
            <div id="trip-followers-pagination" style="display: flex; justify-content: center; align-items: center; gap: 12px; margin-top: 14px;"></div>
        </div>
    `;

    try {
        const followers = await apiFetch(`/api/admin/trips/${currentDetailsTripId}/followers`);
        detailsState.tripFollowers = followers || [];
        renderTripFollowersList();
    } catch (err) {
        document.getElementById('trip-followers-list-container').innerHTML = `
            <div style="color: var(--accent-red); font-size: 13px; text-align: center; padding: 20px;">
                Error: ${err.message}
            </div>
        `;
    }
}

function setTripFollowersSearch(value) {
    detailsState.tripFollowersSearch = value;
    detailsState.tripFollowersPage = 1;
    renderTripFollowersList();
}

function renderTripFollowersList() {
    const listContainer = document.getElementById('trip-followers-list-container');
    const paginationContainer = document.getElementById('trip-followers-pagination');
    const countSpan = document.getElementById('trip-followers-count');
    const clearBtn = document.getElementById('btn-details-remove-all-trip-followers');
    if (!listContainer) return;

    const isRtl = state.language === 'ar';
    const query = detailsState.tripFollowersSearch.toLowerCase();

    const filtered = detailsState.tripFollowers.filter(f => 
        (f.displayName && f.displayName.toLowerCase().includes(query)) ||
        (f.email && f.email.toLowerCase().includes(query))
    );

    countSpan.textContent = filtered.length;

    if (filtered.length > 0) {
        clearBtn.style.display = 'inline-block';
    } else {
        clearBtn.style.display = 'none';
    }

    const pageSize = 5;
    const totalPages = Math.ceil(filtered.length / pageSize) || 1;
    if (detailsState.tripFollowersPage > totalPages) detailsState.tripFollowersPage = totalPages;
    const pageIndex = detailsState.tripFollowersPage - 1;
    const paginated = filtered.slice(pageIndex * pageSize, (pageIndex + 1) * pageSize);

    let html = '';
    if (paginated.length === 0) {
        html = `
            <div style="color: var(--text-muted); font-size: 13px; text-align: center; padding: 30px 10px;">
                ${isRtl ? 'لا يوجد متابعون.' : 'No followers found.'}
            </div>
        `;
    } else {
        paginated.forEach(follower => {
            const initial = follower.displayName ? follower.displayName[0].toUpperCase() : 'U';
            const dateStr = follower.followedAt ? new Date(follower.followedAt).toLocaleDateString([], { month: 'short', day: 'numeric' }) : '';
            
            let statusText = follower.personalStatus;
            if (follower.personalStatus === 'Following' || follower.personalStatus === 0) {
                statusText = isRtl ? 'متابع' : 'Following';
            } else if (follower.personalStatus === 'Started' || follower.personalStatus === 1) {
                statusText = isRtl ? 'صعد القطار' : 'Boarded';
            } else if (follower.personalStatus === 'Ended' || follower.personalStatus === 2) {
                statusText = isRtl ? 'مكتمل' : 'Completed';
            }

            html += `
                <div style="display: flex; align-items: center; justify-content: space-between; padding: 10px 14px; border-radius: 8px; border: 1px solid var(--border-color); background: rgba(255,255,255,0.015); gap: 12px;">
                    <div style="display: flex; align-items: center; gap: 10px; min-width: 0;">
                        <div style="width: 32px; height: 32px; border-radius: 50%; background: var(--accent-orange); color: white; display: flex; align-items: center; justify-content: center; font-weight: 700; font-size: 13px; flex-shrink: 0;">
                            ${initial}
                        </div>
                        <div style="min-width: 0;">
                            <div style="font-weight: 600; color: white; font-size: 13px; overflow: hidden; text-overflow: ellipsis; white-space: nowrap;">${follower.displayName}</div>
                            <div style="font-size: 11px; color: var(--text-secondary); overflow: hidden; text-overflow: ellipsis; white-space: nowrap;">${follower.email}</div>
                        </div>
                    </div>
                    <div style="display: flex; align-items: center; gap: 12px; flex-shrink: 0;">
                        <div style="display: flex; flex-direction: column; align-items: flex-end; gap: 2px;">
                            <span class="status-pill arrived" style="font-size: 9px; padding: 2px 6px;">
                                ${statusText}
                            </span>
                            <span style="font-size: 10px; color: var(--text-muted);">${dateStr}</span>
                        </div>
                        <button class="action-btn delete" onclick="detailsRemoveTripFollower('${follower.userId}')" title="${isRtl ? 'إزالة' : 'Remove'}" style="padding: 6px; height: auto;">
                            <i class="fa-solid fa-trash-can"></i>
                        </button>
                    </div>
                </div>
            `;
        });
    }

    listContainer.innerHTML = html;

    if (totalPages > 1) {
        paginationContainer.innerHTML = `
            <button class="btn btn-secondary" onclick="changeTripFollowersPage(-1)" ${detailsState.tripFollowersPage === 1 ? 'disabled' : ''} style="padding: 4px 10px; font-size: 11px; height: 26px; min-width: auto; margin: 0;">
                ${isRtl ? 'السابق' : 'Prev'}
            </button>
            <span style="font-size: 11px; color: var(--text-secondary);">
                ${isRtl ? `صفحة ${detailsState.tripFollowersPage} من ${totalPages}` : `Page ${detailsState.tripFollowersPage} of ${totalPages}`}
            </span>
            <button class="btn btn-secondary" onclick="changeTripFollowersPage(1)" ${detailsState.tripFollowersPage === totalPages ? 'disabled' : ''} style="padding: 4px 10px; font-size: 11px; height: 26px; min-width: auto; margin: 0;">
                ${isRtl ? 'التالي' : 'Next'}
            </button>
        `;
    } else {
        paginationContainer.innerHTML = '';
    }
}

function changeTripFollowersPage(dir) {
    detailsState.tripFollowersPage += dir;
    renderTripFollowersList();
}

async function detailsRemoveTripFollower(userId) {
    const confirmMsg = state.language === 'ar'
        ? 'هل أنت متأكد من إزالة هذا المتابع؟'
        : 'Are you sure you want to remove this follower?';
    if (!confirm(confirmMsg)) return;
    
    try {
        await apiFetch(`/api/admin/trips/${currentDetailsTripId}/followers/${userId}`, { method: 'DELETE' });
        await loadTripDetailsFollowers();
        if (state.activeTab === 'trips') loadTrips();
    } catch (err) {
        alert(`Error: ${err.message}`);
    }
}

async function detailsRemoveAllTripFollowers() {
    const confirmMsg = state.language === 'ar'
        ? 'هل أنت متأكد من إزالة جميع المتابعين لهذه الرحلة؟'
        : 'Are you sure you want to remove ALL followers for this trip?';
    if (!confirm(confirmMsg)) return;
    
    try {
        await apiFetch(`/api/admin/trips/${currentDetailsTripId}/followers`, { method: 'DELETE' });
        await loadTripDetailsFollowers();
        if (state.activeTab === 'trips') loadTrips();
    } catch (err) {
        alert(`Error: ${err.message}`);
    }
}

function viewSuggestion(id) {
    const sug = state.suggestions.find(s => s.id === id);
    if (!sug) {
        alert('Suggestion details not found.');
        return;
    }

    const dateStr = new Date(sug.createdAt).toLocaleString();
    let statusClass = 'scheduled';
    let statusText = t('status_new');
    if (sug.status === 1 || sug.status === 'Approved') {
        statusClass = 'arrived';
        statusText = t('modal_suggestion_approve');
    } else if (sug.status === 2 || sug.status === 'Rejected') {
        statusClass = 'cancelled';
        statusText = t('modal_suggestion_reject');
    }

    const sugName = state.language === 'ar' ? (sug.nameAr || sug.nameEn) : (sug.nameEn || sug.nameAr);
    const sugDesc = state.language === 'ar' ? (sug.descriptionAr || sug.descriptionEn) : (sug.descriptionEn || sug.descriptionAr);
    const sugRoute = state.language === 'ar' ? (sug.routeDescriptionAr || sug.routeDescriptionEn) : (sug.routeDescriptionEn || sug.routeDescriptionAr);

    const alignmentStyle = state.language === 'ar' ? 'direction: rtl; text-align: right;' : 'direction: ltr; text-align: left;';

    const html = `
        <div style="display: flex; flex-direction: column; gap: 16px; ${alignmentStyle}">
            <div style="display: flex; align-items: center; justify-content: space-between; margin-bottom: 8px;">
                <div style="display: flex; align-items: center; gap: 12px;">
                    <span style="font-size: 24px; color: var(--accent-orange);"><i class="fa-solid fa-lightbulb"></i></span>
                    <div>
                        <h3 style="font-size: 18px; margin: 0; color: white;">${t('suggestions')}</h3>
                        <p style="margin: 2px 0 0 0; color: var(--text-secondary); font-size: 14px;">${t('table_col_train_no')}: <strong>${sug.trainNumber}</strong> - ${sugName}</p>
                    </div>
                </div>
                <span class="status-pill ${statusClass}" style="font-size: 11px; font-weight: 700;">
                    ${statusText}
                </span>
            </div>

            <div style="display: grid; grid-template-columns: 1fr; gap: 14px; background: rgba(255,255,255,0.02); padding: 16px; border-radius: 8px; border: 1px solid var(--border-color);">
                <div>
                    <span style="display: block; font-size: 11px; text-transform: uppercase; color: var(--text-secondary); margin-bottom: 4px;">${t('table_col_suggested_by')}</span>
                    <strong style="color: white; font-size: 14px;"><i class="fa-regular fa-user"></i> ${sug.suggestedByName}</strong>
                </div>
                <div>
                    <span style="display: block; font-size: 11px; text-transform: uppercase; color: var(--text-secondary); margin-bottom: 4px;">${t('table_col_date')}</span>
                    <strong style="color: white; font-size: 13px;">${dateStr}</strong>
                </div>
            </div>

            <div style="background: rgba(255,255,255,0.02); padding: 16px; border-radius: 8px; border: 1px solid var(--border-color); display: flex; flex-direction: column; gap: 12px;">
                <div>
                    <span style="display: block; font-size: 11px; text-transform: uppercase; color: var(--text-secondary); margin-bottom: 4px;">${t('table_col_description')}</span>
                    <p style="color: white; font-size: 14px; margin: 0; line-height: 1.6;">${sugDesc || '-'}</p>
                </div>
            </div>

            <div style="background: rgba(255,255,255,0.02); padding: 16px; border-radius: 8px; border: 1px solid var(--border-color); display: flex; flex-direction: column; gap: 12px;">
                <div>
                    <span style="display: block; font-size: 11px; text-transform: uppercase; color: var(--text-secondary); margin-bottom: 4px;">${t('table_col_proposed_route')}</span>
                    <p style="color: white; font-size: 14px; margin: 0; line-height: 1.6;">${sugRoute || '-'}</p>
                </div>
            </div>

            ${sug.adminNotes ? `
            <div style="background: rgba(239, 68, 68, 0.05); border: 1px solid rgba(239, 68, 68, 0.15); padding: 16px; border-radius: 8px;">
                <span style="display: block; font-size: 11px; text-transform: uppercase; color: #fca5a5; margin-bottom: 4px;">Admin Review Notes</span>
                <p style="color: #fca5a5; font-size: 14px; margin: 0; line-height: 1.6;">${sug.adminNotes}</p>
            </div>
            ` : ''}
        </div>
    `;

    showDetailsModal(t('modal_details_title'), html, false);
}

// ==========================================================================
// 🔍 LOST & FOUND MODERATION LOGIC
// ==========================================================================

state.lostFoundPosts = [];

// Event listeners for filters
function setupLostFoundEventListeners() {
    const searchInput = document.getElementById('lostfound-search');
    const typeFilter = document.getElementById('lostfound-type-filter');
    const statusFilter = document.getElementById('lostfound-status-filter');

    if (searchInput) searchInput.addEventListener('input', renderLostFoundPosts);
    if (typeFilter) typeFilter.addEventListener('change', renderLostFoundPosts);
    if (statusFilter) statusFilter.addEventListener('change', renderLostFoundPosts);
}

// Ensure event listeners are set up
setupLostFoundEventListeners();

async function loadLostFoundPosts() {
    const tbody = document.getElementById('lostfound-list');
    if (!tbody) return;
    
    tbody.innerHTML = `<tr><td colspan="8" class="loading-cell">${t('loading_lostfound')}</td></tr>`;
    
    try {
        const posts = await apiFetch('/api/admin/lost-found/posts');
        state.lostFoundPosts = posts || [];
        renderLostFoundPosts();
    } catch (err) {
        console.error('Failed to load lost & found posts:', err);
        tbody.innerHTML = `<tr><td colspan="8" class="error-cell"><i class="fa-solid fa-triangle-exclamation"></i> Error: ${err.message}</td></tr>`;
    }
}

function renderLostFoundPosts() {
    const tbody = document.getElementById('lostfound-list');
    if (!tbody) return;
    
    const searchQuery = (document.getElementById('lostfound-search')?.value || '').toLowerCase().trim();
    const typeFilter = document.getElementById('lostfound-type-filter')?.value || '';
    const statusFilter = document.getElementById('lostfound-status-filter')?.value || '';
    
    const filtered = state.lostFoundPosts.filter(post => {
        const matchesSearch = !searchQuery || 
            post.title.toLowerCase().includes(searchQuery) || 
            (post.trainNumber && post.trainNumber.toLowerCase().includes(searchQuery)) ||
            post.authorName.toLowerCase().includes(searchQuery);
            
        const matchesType = !typeFilter || post.type === typeFilter;
        const matchesStatus = !statusFilter || post.status === statusFilter;
        
        return matchesSearch && matchesType && matchesStatus;
    });
    
    if (filtered.length === 0) {
        tbody.innerHTML = `<tr><td colspan="8" style="text-align: center; color: var(--text-muted); padding: 20px;">${t('no_lostfound')}</td></tr>`;
        return;
    }
    
    tbody.innerHTML = filtered.map(post => {
        const dateStr = new Date(post.createdAt).toLocaleDateString([], { month: 'short', day: 'numeric', hour: '2-digit', minute: '2-digit' });
        
        // Map status classes
        let statusClass = 'scheduled';
        if (post.status === 'Published') statusClass = 'arrived';
        else if (post.status === 'Rejected') statusClass = 'cancelled';
        else if (post.status === 'Closed') statusClass = 'delayed';
        
        const statusText = t(`status_${post.status.toLowerCase()}`) || post.status;
        
        const typeBadge = post.type === 'Lost' 
            ? `<span class="badge" style="background: rgba(239, 68, 68, 0.1); color: #f87171; border: 1px solid rgba(239, 68, 68, 0.2); padding: 2px 6px; border-radius: 4px; font-size: 11px;">${t('type_lost')}</span>` 
            : `<span class="badge" style="background: rgba(59, 130, 246, 0.1); color: #60a5fa; border: 1px solid rgba(59, 130, 246, 0.2); padding: 2px 6px; border-radius: 4px; font-size: 11px;">${t('type_found')}</span>`;
            
        const marginStyle = state.language === 'ar' ? 'margin-right: 5px;' : 'margin-left: 5px;';
        return `
            <tr>
                <td>${dateStr}</td>
                <td>${typeBadge}</td>
                <td style="font-weight: 600; color: white;">${post.title}</td>
                <td>${post.trainNumber || '<span class="text-muted">-</span>'}</td>
                <td>${post.authorName}</td>
                <td><span class="status-pill ${statusClass}">${statusText}</span></td>
                <td><i class="fa-regular fa-comment"></i> ${post.comments ? post.comments.length : 0}</td>
                <td class="actions-cell">
                    <button class="btn btn-outline btn-sm" onclick="openLostFoundModal('${post.id}')">
                        <i class="fa-solid fa-shield-halved"></i> ${t('moderate_btn')}
                    </button>
                    <button class="btn btn-outline btn-sm btn-danger" onclick="deleteLostFoundPost('${post.id}')" style="${marginStyle}">
                        <i class="fa-solid fa-trash"></i>
                    </button>
                </td>
            </tr>
        `;
    }).join('');
}

async function deleteLostFoundPost(postId) {
    if (!confirm(t('confirm_delete_post'))) {
        return;
    }
    
    try {
        await apiFetch(`/api/admin/lost-found/posts/${postId}`, { method: 'DELETE' });
        state.lostFoundPosts = state.lostFoundPosts.filter(p => p.id !== postId);
        renderLostFoundPosts();
    } catch (err) {
        alert(`Failed to delete post: ${err.message}`);
    }
}

function openLostFoundModal(postId) {
    const post = state.lostFoundPosts.find(p => p.id === postId);
    if (!post) return;
    
    document.getElementById('mod-post-id').value = post.id;
    document.getElementById('mod-post-title').value = post.title;
    document.getElementById('mod-post-desc').value = post.description;
    document.getElementById('mod-post-type').value = post.type;
    document.getElementById('mod-post-train').value = post.trainNumber || '';
    document.getElementById('mod-post-contact').value = post.contactInfo || '';
    document.getElementById('mod-post-status').value = post.status;
    document.getElementById('new-admin-comment').value = '';
    
    renderModalComments(post.comments || [], post.id);
    
    document.getElementById('lostfound-modal').classList.remove('hidden');
    applyLocalization();
}

function closeLostFoundModal() {
    document.getElementById('lostfound-modal').classList.add('hidden');
}

function renderModalComments(comments, postId) {
    const container = document.getElementById('mod-comments-list');
    if (!container) return;
    
    if (comments.length === 0) {
        container.innerHTML = `<p style="color: var(--text-muted); font-style: italic; text-align: center; margin: auto;">${t('no_comments')}</p>`;
        return;
    }
    
    const marginStyle = state.language === 'ar' ? 'margin-right: 8px;' : 'margin-left: 8px;';
    container.innerHTML = comments.map(comment => {
        const timeStr = new Date(comment.createdAt).toLocaleDateString([], { month: 'short', day: 'numeric', hour: '2-digit', minute: '2-digit' });
        
        const hiddenBadge = comment.isHidden 
            ? `<span class="badge" style="background: rgba(239, 68, 68, 0.15); color: #fca5a5; font-size: 10px; ${marginStyle} padding: 2px 6px; border-radius: 4px; border: 1px solid rgba(239, 68, 68, 0.2);">Hidden</span>` 
            : '';
            
        const hideBtnIcon = comment.isHidden ? 'fa-eye' : 'fa-eye-slash';
        const hideBtnText = comment.isHidden ? 'Unhide' : 'Hide';
        const hideBtnColor = comment.isHidden ? '#34d399' : '#fbbf24';
        
        return `
            <div style="background: rgba(255,255,255,0.02); border: 1px solid var(--border-color); border-radius: 8px; padding: 12px; display: flex; flex-direction: column; gap: 6px;">
                <div style="display: flex; justify-content: space-between; align-items: center; border-bottom: 1px solid rgba(255,255,255,0.05); padding-bottom: 4px;">
                    <div style="display: flex; align-items: center;">
                        <strong style="color: white; font-size: 13px;"><i class="fa-regular fa-user"></i> ${comment.authorName}</strong>
                        ${hiddenBadge}
                    </div>
                    <span style="color: var(--text-muted); font-size: 11px;">${timeStr}</span>
                </div>
                <div style="color: var(--text-primary); font-size: 13px; line-height: 1.5; word-break: break-word;">${comment.content}</div>
                <div style="display: flex; justify-content: flex-end; gap: 12px; margin-top: 4px; border-top: 1px solid rgba(255,255,255,0.02); padding-top: 4px;">
                    <button type="button" class="btn-text" onclick="toggleHideComment('${comment.id}', ${comment.isHidden}, '${postId}')" style="color: ${hideBtnColor}; font-size: 11px; display: flex; align-items: center; gap: 4px; background: none; border: none; cursor: pointer; padding: 0;">
                        <i class="fa-regular ${hideBtnIcon}"></i> ${hideBtnText}
                    </button>
                    <button type="button" class="btn-text" onclick="editCommentContent('${comment.id}', '${comment.content.replace(/'/g, "\\'")}', '${postId}')" style="color: var(--accent-purple); font-size: 11px; display: flex; align-items: center; gap: 4px; background: none; border: none; cursor: pointer; padding: 0;">
                        <i class="fa-regular fa-pen-to-square"></i> Edit
                    </button>
                    <button type="button" class="btn-text" onclick="deleteComment('${comment.id}', '${postId}')" style="color: #f87171; font-size: 11px; display: flex; align-items: center; gap: 4px; background: none; border: none; cursor: pointer; padding: 0;">
                        <i class="fa-regular fa-trash-can"></i> Delete
                    </button>
                </div>
            </div>
        `;
    }).join('');
}

async function saveLostFoundPostModeration() {
    const postId = document.getElementById('mod-post-id').value;
    const title = document.getElementById('mod-post-title').value;
    const description = document.getElementById('mod-post-desc').value;
    const typeText = document.getElementById('mod-post-type').value;
    const trainNumber = document.getElementById('mod-post-train').value;
    const contactInfo = document.getElementById('mod-post-contact').value;
    const statusText = document.getElementById('mod-post-status').value;
    
    // Status Enum mapping for the API
    const statusMap = {
        'New': 0,
        'Published': 1,
        'Rejected': 2,
        'Closed': 3
    };

    // Type Enum mapping for the API
    const typeMap = {
        'Lost': 0,
        'Found': 1
    };
    
    try {
        // 1. Update post details
        await apiFetch(`/api/admin/lost-found/posts/${postId}`, {
            method: 'PUT',
            body: JSON.stringify({ 
                title, 
                description, 
                type: typeMap[typeText], 
                trainNumber, 
                contactInfo 
            })
        });

        // 2. Update post status
        const updatedPost = await apiFetch(`/api/admin/lost-found/posts/${postId}/status`, {
            method: 'PUT',
            body: JSON.stringify({ status: statusMap[statusText] })
        });
        
        // Update local cache
        const index = state.lostFoundPosts.findIndex(p => p.id === postId);
        if (index !== -1) {
            state.lostFoundPosts[index] = updatedPost;
        }
        
        closeLostFoundModal();
        renderLostFoundPosts();
    } catch (err) {
        alert(`Failed to save moderation: ${err.message}`);
    }
}

async function addAdminComment() {
    const postId = document.getElementById('mod-post-id').value;
    const contentInput = document.getElementById('new-admin-comment');
    const content = contentInput.value.trim();
    
    if (!content) return;
    
    try {
        const comment = await apiFetch(`/api/lost-found/${postId}/comments`, {
            method: 'POST',
            body: JSON.stringify({ content })
        });
        
        // Update local cache comment list
        const post = state.lostFoundPosts.find(p => p.id === postId);
        if (post) {
            if (!post.comments) post.comments = [];
            post.comments.push(comment);
            renderModalComments(post.comments, postId);
            renderLostFoundPosts(); // Refresh comment count in table
        }
        
        contentInput.value = '';
    } catch (err) {
        alert(`Failed to add comment: ${err.message}`);
    }
}

async function toggleHideComment(commentId, isCurrentlyHidden, postId) {
    try {
        const updatedComment = await apiFetch(`/api/admin/lost-found/comments/${commentId}/hide`, {
            method: 'PUT',
            body: JSON.stringify({ isHidden: !isCurrentlyHidden })
        });
        
        // Update local cache
        const post = state.lostFoundPosts.find(p => p.id === postId);
        if (post && post.comments) {
            const commentIndex = post.comments.findIndex(c => c.id === commentId);
            if (commentIndex !== -1) {
                post.comments[commentIndex] = updatedComment;
                renderModalComments(post.comments, postId);
            }
        }
    } catch (err) {
        alert(`Failed to update comment status: ${err.message}`);
    }
}

async function editCommentContent(commentId, currentContent, postId) {
    const newContent = prompt(t('edit_comment_prompt'), currentContent);
    if (newContent === null) return; // Cancelled
    
    const content = newContent.trim();
    if (!content) {
        alert(t('err_comment_empty'));
        return;
    }
    
    try {
        const updatedComment = await apiFetch(`/api/admin/lost-found/comments/${commentId}`, {
            method: 'PUT',
            body: JSON.stringify({ content })
        });
        
        // Update local cache
        const post = state.lostFoundPosts.find(p => p.id === postId);
        if (post && post.comments) {
            const commentIndex = post.comments.findIndex(c => c.id === commentId);
            if (commentIndex !== -1) {
                post.comments[commentIndex] = updatedComment;
                renderModalComments(post.comments, postId);
            }
        }
    } catch (err) {
        alert(`Failed to edit comment: ${err.message}`);
    }
}

async function deleteComment(commentId, postId) {
    if (!confirm(t('confirm_delete_comment'))) return;
    
    try {
        await apiFetch(`/api/admin/lost-found/comments/${commentId}`, {
            method: 'DELETE'
        });
        
        // Update local cache
        const post = state.lostFoundPosts.find(p => p.id === postId);
        if (post && post.comments) {
            post.comments = post.comments.filter(c => c.id !== commentId);
            renderModalComments(post.comments, postId);
            renderLostFoundPosts(); // Refresh comment count in table
        }
    } catch (err) {
        alert(`Failed to delete comment: ${err.message}`);
    }
}

// ==========================================================================
// 🌓 THEME TOGGLE LOGIC (Light / Dark Mode)
// ==========================================================================
function initTheme() {
    const savedTheme = localStorage.getItem('witt_admin_theme') || 'dark';
    if (savedTheme === 'light') {
        document.body.classList.add('light-theme');
        updateThemeIcon(true);
    } else {
        document.body.classList.remove('light-theme');
        updateThemeIcon(false);
    }
}

function toggleTheme() {
    const isLight = document.body.classList.toggle('light-theme');
    localStorage.setItem('witt_admin_theme', isLight ? 'light' : 'dark');
    updateThemeIcon(isLight);
    
    // Update theme toggle text label inside sidebar button
    const themeTextEl = document.querySelector('#theme-toggle-btn span');
    if (themeTextEl) {
        themeTextEl.textContent = isLight ? t('theme_dark') : t('theme_light');
    }
}

function updateThemeIcon(isLight) {
    const themeIcon = document.getElementById('theme-icon');
    const themeBtn = document.getElementById('theme-toggle-btn');
    const sidebarLogo = document.getElementById('sidebar-logo-img');
    if (sidebarLogo) {
        sidebarLogo.src = isLight ? '/logo-dark.png' : '/logo-light.png';
    }
    
    // Update PNG favicons dynamically to match active theme
    const pngFavicons = document.querySelectorAll('link[rel="icon"][type="image/png"]');
    pngFavicons.forEach(fav => {
        fav.removeAttribute('media');
        fav.href = isLight ? '/favicon-dark.png' : '/favicon-light.png';
    });

    if (!themeIcon) return;
    
    if (isLight) {
        themeIcon.className = 'fa-solid fa-moon';
        if (themeBtn) themeBtn.title = t('theme_dark');
    } else {
        themeIcon.className = 'fa-solid fa-sun';
        if (themeBtn) themeBtn.title = t('theme_light');
    }
}

function bindThemeToggleListener() {
    const toggleBtn = document.getElementById('theme-toggle-btn');
    if (toggleBtn) {
        toggleBtn.addEventListener('click', toggleTheme);
    }
}

function bindLangToggleListener() {
    const toggleBtn = document.getElementById('lang-toggle-btn');
    if (toggleBtn) {
        toggleBtn.addEventListener('click', toggleLanguage);
    }
}

async function uploadAvatarFile(file) {
    const formData = new FormData();
    formData.append('file', file);

    try {
        const response = await apiFetch('/api/profile/upload-avatar', {
            method: 'POST',
            body: formData
        });
        
        if (response) {
            document.getElementById('profile-avatar-url').value = response;
            const avatarDisplay = document.getElementById('profile-avatar-display');
            if (avatarDisplay) {
                avatarDisplay.innerHTML = `<img src="${response}" alt="Avatar" style="width: 100%; height: 100%; border-radius: 50%; object-fit: cover;">`;
            }
            
            // Update preview inside modal
            const modalPreview = document.getElementById('modal-avatar-preview');
            if (modalPreview) {
                modalPreview.innerHTML = `<img src="${response}" alt="Avatar" style="width: 100%; height: 100%; border-radius: 50%; object-fit: cover;">`;
            }
            
            // Show remove button inside modal
            const removeBtn = document.getElementById('modal-remove-avatar-btn');
            if (removeBtn) removeBtn.classList.remove('hidden');
        }
    } catch (err) {
        alert(state.language === 'ar' ? `فشل تحميل الصورة: ${err.message}` : `Failed to upload image: ${err.message}`);
    }
}

async function loadProfile() {
    try {
        const user = await apiFetch('/api/profile');
        if (user) {
            state.userProfile = user; // cache it
            
            // Populate form fields
            const nameInput = document.getElementById('profile-display-name');
            const bioInput = document.getElementById('profile-bio');
            const avatarInput = document.getElementById('profile-avatar-url');
            
            if (nameInput) nameInput.value = user.displayName || '';
            if (bioInput) bioInput.value = user.bio || '';
            if (avatarInput) avatarInput.value = user.avatarUrl || '';
            
            // Populate display fields
            const nameDisplay = document.getElementById('profile-name-display');
            const emailDisplay = document.getElementById('profile-email-display');
            if (nameDisplay) nameDisplay.textContent = user.displayName || 'Admin User';
            if (emailDisplay) emailDisplay.textContent = user.email || 'admin@whereisthetrain.com';
            
            const adminName = document.getElementById('admin-name');
            const dropdownAdminName = document.getElementById('dropdown-admin-name');
            const adminEmail = document.getElementById('admin-email');
            if (adminName) adminName.textContent = user.displayName || 'Admin User';
            if (dropdownAdminName) dropdownAdminName.textContent = user.displayName || 'Admin User';
            if (adminEmail) adminEmail.textContent = user.email || 'admin@whereisthetrain.com';
            
            const avatarDisplay = document.getElementById('profile-avatar-display');
            if (avatarDisplay) {
                if (user.avatarUrl) {
                    avatarDisplay.innerHTML = `<img src="${user.avatarUrl}" alt="Avatar" style="width: 100%; height: 100%; border-radius: 50%; object-fit: cover;">`;
                } else {
                    avatarDisplay.innerHTML = `<i class="fa-solid fa-user-shield"></i>`;
                }
            }
            
            const adminAvatarLink = document.getElementById('admin-avatar-link');
            if (adminAvatarLink) {
                if (user.avatarUrl) {
                    adminAvatarLink.innerHTML = `<img src="${user.avatarUrl}" alt="Avatar" style="width: 100%; height: 100%; border-radius: 50%; object-fit: cover;">`;
                } else {
                    adminAvatarLink.innerHTML = `<i class="fa-solid fa-user-shield"></i>`;
                }
            }

            const modalPreview = document.getElementById('modal-avatar-preview');
            if (modalPreview) {
                if (user.avatarUrl) {
                    modalPreview.innerHTML = `<img src="${user.avatarUrl}" alt="Avatar" style="width: 100%; height: 100%; border-radius: 50%; object-fit: cover;">`;
                } else {
                    modalPreview.innerHTML = `<i class="fa-solid fa-user-shield"></i>`;
                }
            }
        }
    } catch (err) {
        console.error('Error loading profile:', err);
    }
}

function openAvatarModal() {
    const modal = document.getElementById('avatar-edit-modal');
    if (!modal) return;
    modal.classList.remove('hidden');
    
    // Sync preview inside modal with current hidden input value
    const currentAvatarUrl = document.getElementById('profile-avatar-url').value;
    const modalPreview = document.getElementById('modal-avatar-preview');
    if (modalPreview) {
        if (currentAvatarUrl) {
            modalPreview.innerHTML = `<img src="${currentAvatarUrl}" alt="Avatar" style="width: 100%; height: 100%; border-radius: 50%; object-fit: cover;">`;
        } else {
            modalPreview.innerHTML = `<i class="fa-solid fa-user-shield"></i>`;
        }
    }
    
    // Show/hide remove button based on if avatar is currently set
    const removeBtn = document.getElementById('modal-remove-avatar-btn');
    if (removeBtn) {
        if (currentAvatarUrl) {
            removeBtn.classList.remove('hidden');
        } else {
            removeBtn.classList.add('hidden');
        }
    }
}

function closeAvatarModal() {
    const modal = document.getElementById('avatar-edit-modal');
    if (modal) modal.classList.add('hidden');
}

async function handleModalAvatarUpload(e) {
    const file = e.target.files[0];
    if (!file) return;
    await uploadAvatarFile(file);
}

function removeAvatarPhoto() {
    // Clear hidden input
    document.getElementById('profile-avatar-url').value = '';
    
    // Set both previews to default icon
    const avatarDisplay = document.getElementById('profile-avatar-display');
    if (avatarDisplay) {
        avatarDisplay.innerHTML = `<i class="fa-solid fa-user-shield"></i>`;
    }
    
    const modalPreview = document.getElementById('modal-avatar-preview');
    if (modalPreview) {
        modalPreview.innerHTML = `<i class="fa-solid fa-user-shield"></i>`;
    }
    
    // Hide remove button
    const removeBtn = document.getElementById('modal-remove-avatar-btn');
    if (removeBtn) removeBtn.classList.add('hidden');
    
    // Reset file input value so upload event is triggered if the same file is selected again
    const fileInput = document.getElementById('modal-avatar-file-input');
    if (fileInput) fileInput.value = '';
}

// ==========================================================================
// 🚨 SERVICE DISRUPTIONS & USER MANAGEMENT LOGIC
// ==========================================================================

async function loadUsers() {
    const tableBody = document.getElementById('users-list');
    if (!tableBody) return;
    
    tableBody.innerHTML = `<tr><td colspan="5" class="loading-cell">Loading users...</td></tr>`;
    
    try {
        const users = await apiFetch('/api/admin/users');
        tableBody.innerHTML = '';
        
        if (users.length === 0) {
            tableBody.innerHTML = `<tr><td colspan="5" class="no-data-cell">${state.language === 'ar' ? 'لا يوجد مستخدمين مسجلين.' : 'No registered users found.'}</td></tr>`;
            return;
        }
        
        users.forEach(user => {
            const tr = document.createElement('tr');
            
            const roleText = user.role === 1 
                ? (state.language === 'ar' ? 'مشرف' : 'Admin') 
                : (state.language === 'ar' ? 'مستخدم' : 'User');
                
            const statusText = user.isSuspended 
                ? t('status_suspended') 
                : t('status_active_user');
                
            const statusClass = user.isSuspended ? 'suspended' : 'active';
            
            const suspendActionTitle = user.isSuspended 
                ? (state.language === 'ar' ? 'تفعيل الحساب' : 'Activate User') 
                : (state.language === 'ar' ? 'إيقاف الحساب' : 'Suspend User');
                
            const roleActionTitle = user.role === 1 
                ? (state.language === 'ar' ? 'تغيير إلى مستخدم' : 'Demote to User') 
                : (state.language === 'ar' ? 'ترقية إلى مشرف' : 'Promote to Admin');
                
            tr.innerHTML = `
                <td style="font-weight: 600; color: white;">${user.displayName}</td>
                <td><code>${user.email}</code></td>
                <td><span class="status-pill ${user.role === 1 ? 'departed' : 'scheduled'}">${roleText}</span></td>
                <td><span class="status-pill ${statusClass}">${statusText}</span></td>
                <td class="actions-column">
                    <button class="action-btn view" onclick="openUserFollowingsModal('${user.id}', '${user.displayName.replace(/'/g, "\\'")}')" title="${state.language === 'ar' ? 'عرض الاشتراكات' : 'View Followings'}"><i class="fa-solid fa-list-ul"></i></button>
                    <button class="action-btn edit" onclick="changeUserRole('${user.id}', ${user.role === 1 ? 0 : 1})" title="${roleActionTitle}"><i class="fa-solid fa-user-shield"></i></button>
                    <button class="action-btn delete" onclick="toggleUserSuspension('${user.id}', ${user.isSuspended})" title="${suspendActionTitle}"><i class="fa-solid ${user.isSuspended ? 'fa-user-check' : 'fa-user-slash'}"></i></button>
                </td>
            `;
            tableBody.appendChild(tr);
        });
    } catch (err) {
        tableBody.innerHTML = `<tr><td colspan="5" class="no-data-cell" style="color:var(--accent-red)">Error loading users: ${err.message}</td></tr>`;
    }
}

async function changeUserRole(userId, newRole) {
    const confirmMsg = state.language === 'ar' 
        ? 'هل أنت متأكد من رغبتك في تغيير دور هذا المستخدم؟' 
        : 'Are you sure you want to change this user\'s role?';
    if (!confirm(confirmMsg)) return;
    
    try {
        await apiFetch(`/api/admin/users/${userId}/role`, {
            method: 'PUT',
            body: JSON.stringify({ role: newRole })
        });
        loadUsers();
    } catch (err) {
        alert(`Error updating role: ${err.message}`);
    }
}

async function toggleUserSuspension(userId, currentSuspended) {
    const newSuspended = !currentSuspended;
    const confirmMsg = newSuspended 
        ? (state.language === 'ar' ? 'هل أنت متأكد من إيقاف هذا الحساب؟' : 'Are you sure you want to suspend this user account?')
        : (state.language === 'ar' ? 'هل أنت متأكد من تفعيل هذا الحساب؟' : 'Are you sure you want to activate this user account?');
    if (!confirm(confirmMsg)) return;
    
    try {
        await apiFetch(`/api/admin/users/${userId}/suspend`, {
            method: 'PUT',
            body: JSON.stringify({ isSuspended: newSuspended })
        });
        loadUsers();
    } catch (err) {
        alert(`Error updating suspension status: ${err.message}`);
    }
}

async function loadDisruptions() {
    const tableBody = document.getElementById('disruptions-list');
    if (!tableBody) return;
    
    tableBody.innerHTML = `<tr><td colspan="5" class="loading-cell">Loading alerts...</td></tr>`;
    
    try {
        const disruptions = await apiFetch('/api/admin/disruptions');
        tableBody.innerHTML = '';
        
        if (disruptions.length === 0) {
            tableBody.innerHTML = `<tr><td colspan="5" class="no-data-cell">No alerts found.</td></tr>`;
            return;
        }
        
        disruptions.forEach(alert => {
            const tr = document.createElement('tr');
            
            const dateStr = new Date(alert.createdAt).toLocaleDateString([], { month: 'short', day: 'numeric' }) + ' ' + 
                            new Date(alert.createdAt).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
                            
            const title = state.language === 'ar' ? alert.titleAr : alert.titleEn;
            const affected = alert.affectedLine || '-';
            
            const statusText = alert.isActive 
                ? (state.language === 'ar' ? 'نشط' : 'Active') 
                : (state.language === 'ar' ? 'منتهي' : 'Inactive');
            const statusClass = alert.isActive ? 'delayed' : 'scheduled';
            
            let actionHtml = '';
            if (alert.isActive) {
                const deactText = state.language === 'ar' ? 'إنهاء البث' : 'Deactivate';
                actionHtml = `<button class="action-btn delete" onclick="deactivateDisruption('${alert.id}')" title="${deactText}"><i class="fa-solid fa-power-off"></i></button>`;
            }
            
            tr.innerHTML = `
                <td><code>${dateStr}</code></td>
                <td style="font-weight: 600; color: white;">${title}</td>
                <td>${affected}</td>
                <td><span class="status-pill ${statusClass}">${statusText}</span></td>
                <td class="actions-column">
                    ${actionHtml}
                </td>
            `;
            tableBody.appendChild(tr);
        });
    } catch (err) {
        tableBody.innerHTML = `<tr><td colspan="5" class="no-data-cell" style="color:var(--accent-red)">Error loading disruptions: ${err.message}</td></tr>`;
    }
}

async function deactivateDisruption(id) {
    const confirmMsg = state.language === 'ar' 
        ? 'هل أنت متأكد من إنهاء بث هذا التنبيه؟' 
        : 'Are you sure you want to deactivate this alert?';
    if (!confirm(confirmMsg)) return;
    
    try {
        await apiFetch(`/api/admin/disruptions/${id}/deactivate`, {
            method: 'PUT'
        });
        loadDisruptions();
        fetchAndRenderActiveBanner();
    } catch (err) {
        alert(`Error deactivating alert: ${err.message}`);
    }
}

async function fetchAndRenderActiveBanner() {
    const bannerEl = document.getElementById('service-disruption-banner');
    const bannerContentEl = document.getElementById('disruption-banner-content');
    if (!bannerEl || !bannerContentEl) return;
    
    try {
        const disruptions = await apiFetch('/api/admin/disruptions');
        const active = disruptions.filter(d => d.isActive);
        
        if (active.length === 0) {
            bannerEl.classList.add('hidden');
            bannerContentEl.innerHTML = '';
            return;
        }
        
        bannerEl.classList.remove('hidden');
        
        const bannerHtml = active.map(alert => {
            const title = state.language === 'ar' ? alert.titleAr : alert.titleEn;
            const desc = state.language === 'ar' ? alert.descriptionAr : alert.descriptionEn;
            const lineHtml = alert.affectedLine ? `[${alert.affectedLine}] ` : '';
            return `<strong>${lineHtml}${title}</strong>: ${desc}`;
        }).join(' &nbsp;|&nbsp; ');
        
        bannerContentEl.innerHTML = `
            <marquee behavior="scroll" direction="${state.language === 'ar' ? 'right' : 'left'}" scrollamount="4">
                ${bannerHtml}
            </marquee>
        `;
    } catch (err) {
        console.error('Failed to load disruption banner:', err);
    }
}

async function handleStopsCsvUpload(event) {
    const file = event.target.files[0];
    if (!file) return;
    
    if (!file.name.endsWith('.csv')) {
        alert('Please select a valid CSV file.');
        event.target.value = '';
        return;
    }
    
    const confirmMsg = state.language === 'ar'
        ? `هل أنت متأكد من استيراد المحطات من الملف "${file.name}"؟`
        : `Are you sure you want to import stops from "${file.name}"?`;
    if (!confirm(confirmMsg)) {
        event.target.value = '';
        return;
    }
    
    try {
        const formData = new FormData();
        formData.append('file', file);
        
        const importedCount = await apiFetch('/api/admin/stops/import', {
            method: 'POST',
            body: formData
        });
        
        const successMsg = state.language === 'ar'
            ? `تم استيراد ${importedCount} محطة بنجاح!`
            : `Successfully imported ${importedCount} stops!`;
        alert(successMsg);
        loadStops();
    } catch (err) {
        alert(`Failed to import stops: ${err.message}`);
    } finally {
        event.target.value = '';
    }
}

async function handleTrainsCsvUpload(event) {
    const file = event.target.files[0];
    if (!file) return;
    
    if (!file.name.endsWith('.csv')) {
        alert('Please select a valid CSV file.');
        event.target.value = '';
        return;
    }
    
    const confirmMsg = state.language === 'ar'
        ? `هل أنت متأكد من استيراد مسارات القطارات من الملف "${file.name}"؟`
        : `Are you sure you want to import train routes from "${file.name}"?`;
    if (!confirm(confirmMsg)) {
        event.target.value = '';
        return;
    }
    
    try {
        const formData = new FormData();
        formData.append('file', file);
        
        const importedCount = await apiFetch('/api/admin/trains/import', {
            method: 'POST',
            body: formData
        });
        
        const successMsg = state.language === 'ar'
            ? `تم استيراد ${importedCount} مسار قطار بنجاح!`
            : `Successfully imported ${importedCount} train routes!`;
        alert(successMsg);
        loadTrains();
    } catch (err) {
        alert(`Failed to import train routes: ${err.message}`);
    } finally {
        event.target.value = '';
    }
}

// Bind to window to allow inline html handlers to resolve
window.handleStopsCsvUpload = handleStopsCsvUpload;
window.handleTrainsCsvUpload = handleTrainsCsvUpload;
window.changeUserRole = changeUserRole;
window.toggleUserSuspension = toggleUserSuspension;
window.deactivateDisruption = deactivateDisruption;
window.fetchAndRenderActiveBanner = fetchAndRenderActiveBanner;
window.loadUsers = loadUsers;
window.loadDisruptions = loadDisruptions;

// Register disruption form listener
window.addEventListener('DOMContentLoaded', () => {
    const disruptionForm = document.getElementById('disruption-form');
    if (disruptionForm) {
        disruptionForm.addEventListener('submit', async (e) => {
            e.preventDefault();
            const titleEn = document.getElementById('disruption-title-en').value;
            const titleAr = document.getElementById('disruption-title-ar').value;
            const descEn = document.getElementById('disruption-desc-en').value;
            const descAr = document.getElementById('disruption-desc-ar').value;
            const line = document.getElementById('disruption-line').value;
            
            try {
                await apiFetch('/api/admin/disruptions', {
                    method: 'POST',
                    body: JSON.stringify({
                        titleAr: titleAr,
                        titleEn: titleEn,
                        descriptionAr: descAr,
                        descriptionEn: descEn,
                        affectedLine: line || null
                    })
                });
                disruptionForm.reset();
                loadDisruptions();
                fetchAndRenderActiveBanner();
            } catch (err) {
                alert(`Error broadcasting alert: ${err.message}`);
            }
        });
    }
});

// ==========================================================================
// 🏙️ CITIES CRUD LOGIC
// ==========================================================================
let stateCities = [];

async function loadCities() {
    const tableBody = document.querySelector('#cities-table tbody');
    tableBody.innerHTML = `<tr><td colspan="5" class="loading-cell">${t('loading_cities')}</td></tr>`;
    
    try {
        const cities = await apiFetch('/api/admin/cities');
        stateCities = cities;
        tableBody.innerHTML = '';

        if (cities.length === 0) {
            tableBody.innerHTML = `<tr><td colspan="5" class="no-data-cell">${t('no_cities')}</td></tr>`;
            return;
        }

        cities.forEach(city => {
            const tr = document.createElement('tr');
            
            const govName = state.language === 'ar' ? (city.governorateAr || city.governorateEn) : (city.governorateEn || city.governorateAr);
            
            tr.innerHTML = `
                <td style="font-weight: 600; color: white;">${city.nameAr || ''}</td>
                <td style="font-weight: 600; color: white;">${city.nameEn || ''}</td>
                <td>${govName || ''}</td>
                <td style="text-align: center;"><code>${city.stopsCount || 0}</code></td>
                <td class="actions-column">
                    <button class="action-btn edit" onclick="editCity('${city.id}')" title="Edit"><i class="fa-solid fa-pencil"></i></button>
                    <button class="action-btn delete" onclick="deleteCity('${city.id}')" title="Delete"><i class="fa-solid fa-trash"></i></button>
                </td>
            `;
            tableBody.appendChild(tr);
        });
    } catch (err) {
        tableBody.innerHTML = `<tr><td colspan="5" class="no-data-cell" style="color:var(--accent-red)">Error loading cities: ${err.message}</td></tr>`;
    }
}

async function populateCityGovernoratesDropdown(selectedGovId = '') {
    const select = document.getElementById('city-governorate-id');
    if (!select) return;
    select.innerHTML = `<option value="">${t('modal_city_select_governorate')}</option>`;
    try {
        const govs = await apiFetch('/api/admin/governments');
        govs.forEach(gov => {
            const opt = document.createElement('option');
            opt.value = gov.id;
            opt.textContent = state.language === 'ar' ? gov.nameAr : gov.nameEn;
            if (gov.id === selectedGovId) {
                opt.selected = true;
            }
            select.appendChild(opt);
        });
    } catch (err) {
        console.error('Failed to load governments for cities dropdown:', err);
    }
}

async function openCityModal(city = null) {
    const modal = document.getElementById('city-modal');
    const title = document.getElementById('city-modal-title');
    
    document.getElementById('city-form').reset();
    document.getElementById('city-id').value = '';

    let selectedGovId = '';
    if (city) {
        title.textContent = t('modal_city_edit_title');
        document.getElementById('city-id').value = city.id;
        document.getElementById('city-name-ar').value = city.nameAr || '';
        document.getElementById('city-name-en').value = city.nameEn || '';
        selectedGovId = city.governorateId || '';
    } else {
        title.textContent = t('modal_city_add_title');
    }

    await populateCityGovernoratesDropdown(selectedGovId);

    modal.classList.remove('hidden');
    applyLocalization();
}

function closeCityModal() {
    document.getElementById('city-modal').classList.add('hidden');
}

function editCity(id) {
    const city = stateCities.find(c => c.id === id);
    if (city) openCityModal(city);
}

async function deleteCity(id) {
    if (!confirm(t('confirm_delete_city'))) return;
    try {
        await apiFetch(`/api/admin/cities/${id}`, { method: 'DELETE' });
        loadCities();
    } catch (err) {
        alert(`Error deleting city: ${err.message}`);
    }
}

// Bind to window for onclick attributes
window.loadCities = loadCities;
window.openCityModal = openCityModal;
window.closeCityModal = closeCityModal;
window.editCity = editCity;
window.deleteCity = deleteCity;

// Search/Filter cities
function filterCitiesTable(query) {
    const tableBody = document.querySelector('#cities-table tbody');
    const filtered = stateCities.filter(c => 
        (c.nameAr && c.nameAr.toLowerCase().includes(query)) || 
        (c.nameEn && c.nameEn.toLowerCase().includes(query)) ||
        (c.governorateAr && c.governorateAr.toLowerCase().includes(query)) ||
        (c.governorateEn && c.governorateEn.toLowerCase().includes(query))
    );

    tableBody.innerHTML = '';
    if (filtered.length === 0) {
        tableBody.innerHTML = `<tr><td colspan="5" class="no-data-cell">${t('no_cities')}</td></tr>`;
        return;
    }

    filtered.forEach(city => {
        const tr = document.createElement('tr');
        const govName = state.language === 'ar' ? (city.governorateAr || city.governorateEn) : (city.governorateEn || city.governorateAr);
        tr.innerHTML = `
            <td style="font-weight: 600; color: white;">${city.nameAr || ''}</td>
            <td style="font-weight: 600; color: white;">${city.nameEn || ''}</td>
            <td>${govName || ''}</td>
            <td style="text-align: center;"><code>${city.stopsCount || 0}</code></td>
            <td class="actions-column">
                <button class="action-btn edit" onclick="editCity('${city.id}')" title="Edit"><i class="fa-solid fa-pencil"></i></button>
                <button class="action-btn delete" onclick="deleteCity('${city.id}')" title="Delete"><i class="fa-solid fa-trash"></i></button>
            </td>
        `;
        tableBody.appendChild(tr);
    });
}

// Register City Form and search listeners
window.addEventListener('DOMContentLoaded', () => {
    const cityForm = document.getElementById('city-form');
    if (cityForm) {
        cityForm.addEventListener('submit', async (e) => {
            e.preventDefault();
            const id = document.getElementById('city-id').value;
            const body = {
                nameAr: document.getElementById('city-name-ar').value,
                nameEn: document.getElementById('city-name-en').value,
                governorateId: document.getElementById('city-governorate-id').value
            };

            try {
                if (id) {
                    await apiFetch(`/api/admin/cities/${id}`, {
                        method: 'PUT',
                        body: JSON.stringify(body)
                    });
                } else {
                    await apiFetch('/api/admin/cities', {
                        method: 'POST',
                        body: JSON.stringify(body)
                    });
                }
                closeCityModal();
                loadCities();
            } catch (err) {
                alert(`Error saving city: ${err.message}`);
            }
        });
    }

    const citySearch = document.getElementById('city-search');
    if (citySearch) {
        citySearch.addEventListener('input', (e) => {
            const query = e.target.value.toLowerCase().trim();
            filterCitiesTable(query);
        });
    }
});

// ==========================================================================
// 🏛️ GOVERNMENTS CRUD LOGIC
// ==========================================================================
let stateGovernments = [];

async function loadGovernments() {
    const tableBody = document.querySelector('#governments-table tbody');
    tableBody.innerHTML = `<tr><td colspan="3" class="loading-cell">${t('loading_governments')}</td></tr>`;
    
    try {
        const govs = await apiFetch('/api/admin/governments');
        stateGovernments = govs;
        tableBody.innerHTML = '';

        if (govs.length === 0) {
            tableBody.innerHTML = `<tr><td colspan="3" class="no-data-cell">${t('no_governments')}</td></tr>`;
            return;
        }

        govs.forEach(gov => {
            const tr = document.createElement('tr');
            
            tr.innerHTML = `
                <td style="font-weight: 600; color: white;">${gov.nameAr || ''}</td>
                <td style="font-weight: 600; color: white;">${gov.nameEn || ''}</td>
                <td class="actions-column">
                    <button class="action-btn edit" onclick="editGovernment('${gov.id}')" title="Edit"><i class="fa-solid fa-pencil"></i></button>
                    <button class="action-btn delete" onclick="deleteGovernment('${gov.id}')" title="Delete"><i class="fa-solid fa-trash"></i></button>
                </td>
            `;
            tableBody.appendChild(tr);
        });
    } catch (err) {
        tableBody.innerHTML = `<tr><td colspan="3" class="no-data-cell" style="color:var(--accent-red)">Error loading governments: ${err.message}</td></tr>`;
    }
}

function openGovernmentModal(gov = null) {
    const modal = document.getElementById('government-modal');
    const title = document.getElementById('government-modal-title');
    
    document.getElementById('government-form').reset();
    document.getElementById('government-id').value = '';

    if (gov) {
        title.textContent = t('modal_government_edit_title');
        document.getElementById('government-id').value = gov.id;
        document.getElementById('government-name-ar').value = gov.nameAr || '';
        document.getElementById('government-name-en').value = gov.nameEn || '';
    } else {
        title.textContent = t('modal_government_add_title');
    }

    modal.classList.remove('hidden');
    applyLocalization();
}

function closeGovernmentModal() {
    document.getElementById('government-modal').classList.add('hidden');
}

function editGovernment(id) {
    const gov = stateGovernments.find(g => g.id === id);
    if (gov) openGovernmentModal(gov);
}

async function deleteGovernment(id) {
    if (!confirm(t('confirm_delete_government'))) return;
    try {
        await apiFetch(`/api/admin/governments/${id}`, { method: 'DELETE' });
        loadGovernments();
    } catch (err) {
        alert(`Error deleting government: ${err.message}`);
    }
}

function filterGovernmentsTable(query) {
    const tableBody = document.querySelector('#governments-table tbody');
    const filtered = stateGovernments.filter(g => 
        (g.nameAr && g.nameAr.toLowerCase().includes(query)) || 
        (g.nameEn && g.nameEn.toLowerCase().includes(query))
    );

    tableBody.innerHTML = '';
    if (filtered.length === 0) {
        tableBody.innerHTML = `<tr><td colspan="3" class="no-data-cell">${t('no_governments')}</td></tr>`;
        return;
    }

    filtered.forEach(gov => {
        const tr = document.createElement('tr');
        tr.innerHTML = `
            <td style="font-weight: 600; color: white;">${gov.nameAr || ''}</td>
            <td style="font-weight: 600; color: white;">${gov.nameEn || ''}</td>
            <td class="actions-column">
                <button class="action-btn edit" onclick="editGovernment('${gov.id}')" title="Edit"><i class="fa-solid fa-pencil"></i></button>
                <button class="action-btn delete" onclick="deleteGovernment('${gov.id}')" title="Delete"><i class="fa-solid fa-trash"></i></button>
            </td>
        `;
        tableBody.appendChild(tr);
    });
}

// Bind to window for HTML inline access
window.loadGovernments = loadGovernments;
window.openGovernmentModal = openGovernmentModal;
window.closeGovernmentModal = closeGovernmentModal;
window.editGovernment = editGovernment;
window.deleteGovernment = deleteGovernment;

// ==========================================================================
// ⚙️ SYSTEM SETTINGS & MODERATION LOGIC
// ==========================================================================
async function loadSystemSettings() {
    try {
        const settings = await apiFetch('/api/admin/system-settings');
        document.getElementById('setting-lf-posts-auto').checked = settings.lostFoundPostAutoPublish;
        document.getElementById('setting-lf-comments-auto').checked = settings.lostFoundCommentAutoPublish;
        document.getElementById('setting-live-updates-auto').checked = settings.tripLiveUpdateAutoPublish;
        document.getElementById('setting-live-updates-removal-auto').checked = settings.tripLiveUpdateRemovalAutoApprove;
    } catch (err) {
        console.error('Failed to load system settings:', err);
    }
}

// Pending live updates moderation modal functions
async function openPendingUpdatesModal() {
    const modal = document.getElementById('pending-updates-modal');
    modal.classList.remove('hidden');
    await loadPendingUpdates();
    applyLocalization();
}

function closePendingUpdatesModal() {
    document.getElementById('pending-updates-modal').classList.add('hidden');
}

async function loadPendingUpdates() {
    const tbody = document.getElementById('pending-updates-list');
    if (!tbody) return;
    tbody.innerHTML = `<tr><td colspan="6" class="loading-cell">${t('loading_updates')}</td></tr>`;

    try {
        const updates = await apiFetch('/api/admin/trips/updates/pending');
        tbody.innerHTML = '';

        if (updates.length === 0) {
            tbody.innerHTML = `<tr><td colspan="6" style="text-align: center; color: var(--text-muted); padding: 20px;">No pending updates for review.</td></tr>`;
            return;
        }

        updates.forEach(u => {
            const tr = document.createElement('tr');
            
            const dateStr = new Date(u.createdAt).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }) + ' ' +
                            new Date(u.createdAt).toLocaleDateString([], { month: 'short', day: 'numeric' });

            let detailsHtml = '';
            if (u.statusTag) {
                const tagText = getLookupName('StatusTag', u.statusTag);
                detailsHtml += `<span class="status-pill ${u.statusTag.toLowerCase()}" style="font-size:10px; margin-right:4px;">${tagText}</span>`;
            }
            if (u.crowdState) {
                const crowdText = getLookupName('CrowdLevel', u.crowdState);
                detailsHtml += `<span class="status-pill crowd-${u.crowdState.toLowerCase()}" style="font-size:10px;">${crowdText}</span>`;
            }

            tr.innerHTML = `
                <td><strong>${u.trainNumber}</strong></td>
                <td><code>${u.tripDate}</code></td>
                <td>${u.authorName}</td>
                <td>${u.content}</td>
                <td>
                    <div style="display:flex; flex-direction:column; gap:4px;">
                        <div>${detailsHtml}</div>
                        ${u.latitude ? `<span style="font-size:10px; color:var(--text-secondary);"><i class="fa-solid fa-location-dot"></i> GPS: ${u.latitude.toFixed(4)}, ${u.longitude.toFixed(4)}</span>` : ''}
                    </div>
                </td>
                <td class="actions-column">
                    <button class="action-btn edit" onclick="approveUpdate('${u.id}')" title="Approve" style="background: rgba(16, 185, 129, 0.15); color: #10b981;"><i class="fa-solid fa-check"></i></button>
                    <button class="action-btn delete" onclick="deleteUpdate('${u.id}')" title="Reject / Delete" style="background: rgba(239, 68, 68, 0.15); color: #ef4444;"><i class="fa-solid fa-trash-can"></i></button>
                </td>
            `;
            tbody.appendChild(tr);
        });
    } catch (err) {
        tbody.innerHTML = `<tr><td colspan="6" class="no-data-cell" style="color:var(--accent-red)">Error: ${err.message}</td></tr>`;
    }
}

async function approveUpdate(id) {
    try {
        await apiFetch(`/api/admin/trips/updates/${id}/approve`, { method: 'PUT' });
        alert(t('approve_update_success') || 'Approved!');
        await loadPendingUpdates();
    } catch (err) {
        alert(`Error: ${err.message}`);
    }
}

async function deleteUpdate(id) {
    if (!confirm(t('confirm_delete_update') || 'Are you sure?')) return;
    try {
        await apiFetch(`/api/admin/trips/updates/${id}`, { method: 'DELETE' });
        alert(t('delete_update_success') || 'Deleted!');
        await loadPendingUpdates();
    } catch (err) {
        alert(`Error: ${err.message}`);
    }
}

window.openPendingUpdatesModal = openPendingUpdatesModal;
window.closePendingUpdatesModal = closePendingUpdatesModal;
window.approveUpdate = approveUpdate;
window.deleteUpdate = deleteUpdate;
window.loadSystemSettings = loadSystemSettings;
window.openAvatarModal = openAvatarModal;
window.closeAvatarModal = closeAvatarModal;
window.handleModalAvatarUpload = handleModalAvatarUpload;
window.removeAvatarPhoto = removeAvatarPhoto;

// Register government form and search listeners
window.addEventListener('DOMContentLoaded', () => {
    const govForm = document.getElementById('government-form');
    if (govForm) {
        govForm.addEventListener('submit', async (e) => {
            e.preventDefault();
            const id = document.getElementById('government-id').value;
            const body = {
                nameAr: document.getElementById('government-name-ar').value,
                nameEn: document.getElementById('government-name-en').value
            };

            try {
                if (id) {
                    await apiFetch(`/api/admin/governments/${id}`, {
                        method: 'PUT',
                        body: JSON.stringify(body)
                    });
                } else {
                    await apiFetch('/api/admin/governments', {
                        method: 'POST',
                        body: JSON.stringify(body)
                    });
                }
                closeGovernmentModal();
                loadGovernments();
            } catch (err) {
                alert(`Error saving government: ${err.message}`);
            }
        });
    }

    const govSearch = document.getElementById('government-search');
    if (govSearch) {
        govSearch.addEventListener('input', (e) => {
            const query = e.target.value.toLowerCase().trim();
            filterGovernmentsTable(query);
        });
    }

    const systemForm = document.getElementById('system-settings-form');
    if (systemForm) {
        systemForm.addEventListener('submit', async (e) => {
            e.preventDefault();
            const body = {
                lostFoundPostAutoPublish: document.getElementById('setting-lf-posts-auto').checked,
                lostFoundCommentAutoPublish: document.getElementById('setting-lf-comments-auto').checked,
                tripLiveUpdateAutoPublish: document.getElementById('setting-live-updates-auto').checked,
                tripLiveUpdateRemovalAutoApprove: document.getElementById('setting-live-updates-removal-auto').checked
            };

            try {
                await apiFetch('/api/admin/system-settings', {
                    method: 'PUT',
                    body: JSON.stringify(body)
                });
                alert(t('settings_saved_success'));
            } catch (err) {
                alert(`Error saving settings: ${err.message}`);
            }
        });
    }

    const profileForm = document.getElementById('profile-settings-form');
    if (profileForm) {
        profileForm.addEventListener('submit', async (e) => {
            e.preventDefault();
            const displayName = document.getElementById('profile-display-name').value.trim();
            const bio = document.getElementById('profile-bio').value.trim();
            const avatarUrl = document.getElementById('profile-avatar-url').value.trim();
            
            try {
                const response = await apiFetch('/api/profile', {
                    method: 'PUT',
                    body: JSON.stringify({ displayName, bio, avatarUrl })
                });
                
                if (response) {
                    alert(state.language === 'ar' ? 'تم تحديث الملف الشخصي بنجاح!' : 'Profile updated successfully!');
                    state.user.displayName = displayName;
                    const adminName = document.getElementById('admin-name');
                    const dropdownAdminName = document.getElementById('dropdown-admin-name');
                    if (adminName) adminName.textContent = displayName;
                    if (dropdownAdminName) dropdownAdminName.textContent = displayName;
                    loadProfile();
                }
            } catch (err) {
                alert(`Error: ${err.message}`);
            }
        });
    }

    // Status Tags Form Submit
    const statusTagForm = document.getElementById('status-tag-form');
    if (statusTagForm) {
        statusTagForm.addEventListener('submit', async (e) => {
            e.preventDefault();
            const id = document.getElementById('status-tag-id').value;
            const code = document.getElementById('status-tag-code').value;
            const nameAr = document.getElementById('status-tag-name-ar').value;
            const nameEn = document.getElementById('status-tag-name-en').value;
            
            try {
                if (id) {
                    await apiFetch(`/api/admin/status-tags/${id}`, {
                        method: 'PUT',
                        body: JSON.stringify({ id, nameAr, nameEn })
                    });
                } else {
                    await apiFetch('/api/admin/status-tags', {
                        method: 'POST',
                        body: JSON.stringify({ code, nameAr, nameEn })
                    });
                }
                closeStatusTagModal();
                loadStatusTags();
            } catch (err) {
                alert(`Error saving status tag: ${err.message}`);
            }
        });
    }

    // Status Tag Search
    const statusTagSearch = document.getElementById('status-tag-search');
    if (statusTagSearch) {
        statusTagSearch.addEventListener('input', (e) => {
            const query = e.target.value.toLowerCase().trim();
            filterStatusTagsTable(query);
        });
    }

    // Crowd Levels Form Submit
    const crowdLevelForm = document.getElementById('crowd-level-form');
    if (crowdLevelForm) {
        crowdLevelForm.addEventListener('submit', async (e) => {
            e.preventDefault();
            const id = document.getElementById('crowd-level-id').value;
            const code = document.getElementById('crowd-level-code').value;
            const nameAr = document.getElementById('crowd-level-name-ar').value;
            const nameEn = document.getElementById('crowd-level-name-en').value;
            
            try {
                if (id) {
                    await apiFetch(`/api/admin/crowd-levels/${id}`, {
                        method: 'PUT',
                        body: JSON.stringify({ id, nameAr, nameEn })
                    });
                } else {
                    await apiFetch('/api/admin/crowd-levels', {
                        method: 'POST',
                        body: JSON.stringify({ code, nameAr, nameEn })
                    });
                }
                closeCrowdLevelModal();
                loadCrowdLevels();
            } catch (err) {
                alert(`Error saving crowd level: ${err.message}`);
            }
        });
    }

    // Crowd Level Search
    const crowdLevelSearch = document.getElementById('crowd-level-search');
    if (crowdLevelSearch) {
        crowdLevelSearch.addEventListener('input', (e) => {
            const query = e.target.value.toLowerCase().trim();
            filterCrowdLevelsTable(query);
        });
    }

});

// Lookups Management Functions
async function initLookups() {
    try {
        const [tags, levels] = await Promise.all([
            apiFetch('/api/status-tags'),
            apiFetch('/api/crowd-levels')
        ]);
        state.statusTags = tags?.data || tags || [];
        state.crowdLevels = levels?.data || levels || [];
    } catch (err) {
        console.error('Error initializing lookups:', err);
    }
}

// --- Status Tags ---
async function loadStatusTags() {
    const tableBody = document.getElementById('status-tags-list');
    if (!tableBody) return;
    
    tableBody.innerHTML = `<tr><td colspan="4" class="loading-cell" data-i18n="loading_status_tags">Loading status tags...</td></tr>`;
    
    try {
        const res = await apiFetch('/api/admin/status-tags');
        state.statusTags = res?.data || res || [];
        renderStatusTags(state.statusTags);
    } catch (err) {
        console.error('Error loading status tags:', err);
        tableBody.innerHTML = `<tr><td colspan="4" class="error-cell">Error: ${err.message}</td></tr>`;
    }
}

function renderStatusTags(list) {
    const tableBody = document.getElementById('status-tags-list');
    if (!tableBody) return;
    
    if (list.length === 0) {
        tableBody.innerHTML = `<tr><td colspan="4" style="text-align: center; color: var(--text-muted); font-style: italic;">No status tags found.</td></tr>`;
        return;
    }
    
    tableBody.innerHTML = list.map(l => `
        <tr>
            <td style="font-family: monospace; font-size: 13px; color: var(--accent-purple);">${l.code}</td>
            <td>${l.nameAr}</td>
            <td>${l.nameEn}</td>
            <td class="actions-column">
                <button class="action-btn edit" onclick="openStatusTagModal('${l.id}')" title="${state.language === 'ar' ? 'تعديل' : 'Edit'}">
                    <i class="fa-solid fa-pencil"></i>
                </button>
                <button class="action-btn delete" onclick="deleteStatusTag('${l.id}')" title="${state.language === 'ar' ? 'حذف' : 'Delete'}">
                    <i class="fa-solid fa-trash"></i>
                </button>
            </td>
        </tr>
    `).join('');
}

function openStatusTagModal(id = null) {
    const modal = document.getElementById('status-tag-modal');
    if (!modal) return;
    
    const titleEl = document.getElementById('status-tag-modal-title');
    const form = document.getElementById('status-tag-form');
    
    form.reset();
    document.getElementById('status-tag-id').value = id || '';
    document.getElementById('status-tag-code').disabled = false;

    if (id) {
        titleEl.textContent = state.language === 'ar' ? 'تعديل علامة الحالة' : 'Edit Status Tag';
        const lookup = state.statusTags.find(l => l.id === id);
        if (lookup) {
            document.getElementById('status-tag-code').value = lookup.code;
            document.getElementById('status-tag-code').disabled = true;
            document.getElementById('status-tag-name-ar').value = lookup.nameAr;
            document.getElementById('status-tag-name-en').value = lookup.nameEn;
        }
    } else {
        titleEl.textContent = state.language === 'ar' ? 'إضافة علامة حالة' : 'Add Status Tag';
    }
    
    modal.classList.remove('hidden');
}

function closeStatusTagModal() {
    const modal = document.getElementById('status-tag-modal');
    if (modal) modal.classList.add('hidden');
}

async function deleteStatusTag(id) {
    if (!confirm(state.language === 'ar' ? 'هل أنت متأكد من حذف علامة الحالة هذه؟' : 'Are you sure you want to delete this status tag?')) return;
    try {
        await apiFetch(`/api/admin/status-tags/${id}`, { method: 'DELETE' });
        loadStatusTags();
        initLookups(); // Refresh cached list
    } catch (err) {
        alert(`Error: ${err.message}`);
    }
}

function filterStatusTagsTable(query) {
    let filtered = state.statusTags;
    if (query) {
        filtered = filtered.filter(l => 
            l.code.toLowerCase().includes(query) ||
            l.nameAr.toLowerCase().includes(query) ||
            l.nameEn.toLowerCase().includes(query)
        );
    }
    renderStatusTags(filtered);
}

// --- Crowd Levels ---
async function loadCrowdLevels() {
    const tableBody = document.getElementById('crowd-levels-list');
    if (!tableBody) return;
    
    tableBody.innerHTML = `<tr><td colspan="4" class="loading-cell" data-i18n="loading_crowd_levels">Loading crowd levels...</td></tr>`;
    
    try {
        const res = await apiFetch('/api/admin/crowd-levels');
        state.crowdLevels = res?.data || res || [];
        renderCrowdLevels(state.crowdLevels);
    } catch (err) {
        console.error('Error loading crowd levels:', err);
        tableBody.innerHTML = `<tr><td colspan="4" class="error-cell">Error: ${err.message}</td></tr>`;
    }
}

function renderCrowdLevels(list) {
    const tableBody = document.getElementById('crowd-levels-list');
    if (!tableBody) return;
    
    if (list.length === 0) {
        tableBody.innerHTML = `<tr><td colspan="4" style="text-align: center; color: var(--text-muted); font-style: italic;">No crowd levels found.</td></tr>`;
        return;
    }
    
    tableBody.innerHTML = list.map(l => `
        <tr>
            <td style="font-family: monospace; font-size: 13px; color: var(--accent-purple);">${l.code}</td>
            <td>${l.nameAr}</td>
            <td>${l.nameEn}</td>
            <td class="actions-column">
                <button class="action-btn edit" onclick="openCrowdLevelModal('${l.id}')" title="${state.language === 'ar' ? 'تعديل' : 'Edit'}">
                    <i class="fa-solid fa-pencil"></i>
                </button>
                <button class="action-btn delete" onclick="deleteCrowdLevel('${l.id}')" title="${state.language === 'ar' ? 'حذف' : 'Delete'}">
                    <i class="fa-solid fa-trash"></i>
                </button>
            </td>
        </tr>
    `).join('');
}

function openCrowdLevelModal(id = null) {
    const modal = document.getElementById('crowd-level-modal');
    if (!modal) return;
    
    const titleEl = document.getElementById('crowd-level-modal-title');
    const form = document.getElementById('crowd-level-form');
    
    form.reset();
    document.getElementById('crowd-level-id').value = id || '';
    document.getElementById('crowd-level-code').disabled = false;

    if (id) {
        titleEl.textContent = state.language === 'ar' ? 'تعديل مستوى الازدحام' : 'Edit Crowd Level';
        const lookup = state.crowdLevels.find(l => l.id === id);
        if (lookup) {
            document.getElementById('crowd-level-code').value = lookup.code;
            document.getElementById('crowd-level-code').disabled = true;
            document.getElementById('crowd-level-name-ar').value = lookup.nameAr;
            document.getElementById('crowd-level-name-en').value = lookup.nameEn;
        }
    } else {
        titleEl.textContent = state.language === 'ar' ? 'إضافة مستوى ازدحام' : 'Add Crowd Level';
    }
    
    modal.classList.remove('hidden');
}

function closeCrowdLevelModal() {
    const modal = document.getElementById('crowd-level-modal');
    if (modal) modal.classList.add('hidden');
}

async function deleteCrowdLevel(id) {
    if (!confirm(state.language === 'ar' ? 'هل أنت متأكد من حذف مستوى الازدحام هذا؟' : 'Are you sure you want to delete this crowd level?')) return;
    try {
        await apiFetch(`/api/admin/crowd-levels/${id}`, { method: 'DELETE' });
        loadCrowdLevels();
        initLookups(); // Refresh cached list
    } catch (err) {
        alert(`Error: ${err.message}`);
    }
}

function filterCrowdLevelsTable(query) {
    let filtered = state.crowdLevels;
    if (query) {
        filtered = filtered.filter(l => 
            l.code.toLowerCase().includes(query) ||
            l.nameAr.toLowerCase().includes(query) ||
            l.nameEn.toLowerCase().includes(query)
        );
    }
    renderCrowdLevels(filtered);
}

// Expose functions globally
window.openStatusTagModal = openStatusTagModal;
window.closeStatusTagModal = closeStatusTagModal;
window.deleteStatusTag = deleteStatusTag;
window.loadStatusTags = loadStatusTags;

window.openCrowdLevelModal = openCrowdLevelModal;
window.closeCrowdLevelModal = closeCrowdLevelModal;
window.deleteCrowdLevel = deleteCrowdLevel;
window.loadCrowdLevels = loadCrowdLevels;

window.initLookups = initLookups;

// ==========================================================================
// FOLLOWERS & FOLLOWINGS MANAGEMENT
// ==========================================================================
let currentFollowingsUserId = null;
let currentFollowingsActiveTab = 'trains';
let currentTrainFollowersTrainId = null;
let currentTripFollowersTripId = null;

function formatDaysOfWeek(days) {
    if (!days || days.length === 0) return '-';
    const dayNames = state.language === 'ar' 
        ? ['الأحد', 'الاثنين', 'الثلاثاء', 'الأربعاء', 'الخميس', 'الجمعة', 'السبت']
        : ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'];
    return days.map(d => dayNames[d]).join(', ');
}

async function openUserFollowingsModal(userId, displayName) {
    currentFollowingsUserId = userId;
    currentFollowingsActiveTab = 'trains';
    document.getElementById('user-followings-title').innerText = `${state.language === 'ar' ? 'اشتراكات' : 'Followings of'} ${displayName}`;
    
    // Reset tabs
    document.getElementById('btn-following-trains').classList.add('active');
    document.getElementById('btn-following-trips').classList.remove('active');
    document.getElementById('following-trains-section').classList.remove('hidden');
    document.getElementById('following-trips-section').classList.add('hidden');
    
    document.getElementById('user-followings-modal').classList.remove('hidden');
    await loadUserFollowings();
}

function closeUserFollowingsModal() {
    document.getElementById('user-followings-modal').classList.add('hidden');
    currentFollowingsUserId = null;
}

function switchFollowingTab(tab) {
    currentFollowingsActiveTab = tab;
    if (tab === 'trains') {
        document.getElementById('btn-following-trains').classList.add('active');
        document.getElementById('btn-following-trips').classList.remove('active');
        document.getElementById('following-trains-section').classList.remove('hidden');
        document.getElementById('following-trips-section').classList.add('hidden');
    } else {
        document.getElementById('btn-following-trains').classList.remove('active');
        document.getElementById('btn-following-trips').classList.add('active');
        document.getElementById('following-trains-section').classList.add('hidden');
        document.getElementById('following-trips-section').classList.remove('hidden');
    }
}

async function loadUserFollowings() {
    if (!currentFollowingsUserId) return;
    
    const trainsList = document.getElementById('following-trains-list');
    const tripsList = document.getElementById('following-trips-list');
    
    trainsList.innerHTML = `<tr><td colspan="4" class="loading-cell">${state.language === 'ar' ? 'جاري التحميل...' : 'Loading followed trains...'}</td></tr>`;
    tripsList.innerHTML = `<tr><td colspan="4" class="loading-cell">${state.language === 'ar' ? 'جاري التحميل...' : 'Loading followed trips...'}</td></tr>`;
    
    try {
        const followings = await apiFetch(`/api/admin/users/${currentFollowingsUserId}/following`);
        
        // Render Trains
        trainsList.innerHTML = '';
        if (!followings.followedTrains || followings.followedTrains.length === 0) {
            trainsList.innerHTML = `<tr><td colspan="4" class="no-data-cell">${state.language === 'ar' ? 'لا توجد قطارات متبعة' : 'No followed trains.'}</td></tr>`;
        } else {
            followings.followedTrains.forEach(train => {
                const tr = document.createElement('tr');
                const trainName = state.language === 'ar' ? (train.trainNameAr || train.trainNameEn) : (train.trainNameEn || train.trainNameAr);
                tr.innerHTML = `
                    <td><strong>${train.trainNumber}</strong></td>
                    <td>${trainName || ''}</td>
                    <td>${formatDaysOfWeek(train.daysOfWeek)}</td>
                    <td class="actions-column">
                        <button class="action-btn delete" onclick="removeUserFollowedTrain('${train.trainId}')" title="${state.language === 'ar' ? 'إلغاء المتابعة' : 'Unfollow'}"><i class="fa-solid fa-trash-can"></i></button>
                    </td>
                `;
                trainsList.appendChild(tr);
            });
        }
        
        // Render Trips
        tripsList.innerHTML = '';
        if (!followings.followedTrips || followings.followedTrips.length === 0) {
            tripsList.innerHTML = `<tr><td colspan="4" class="no-data-cell">${state.language === 'ar' ? 'لا توجد رحلات متبعة' : 'No followed trips.'}</td></tr>`;
        } else {
            followings.followedTrips.forEach(trip => {
                const tr = document.createElement('tr');
                const trainName = state.language === 'ar' ? (trip.trainNameAr || trip.trainNameEn) : (trip.trainNameEn || trip.trainNameAr);
                tr.innerHTML = `
                    <td><code>${trip.tripDate}</code></td>
                    <td><strong>${trip.trainNumber} - ${trainName || ''}</strong></td>
                    <td><span class="status-pill arrived">${trip.personalStatus}</span></td>
                    <td class="actions-column">
                        <button class="action-btn delete" onclick="removeUserFollowedTrip('${trip.tripId}')" title="${state.language === 'ar' ? 'إلغاء المتابعة' : 'Unfollow'}"><i class="fa-solid fa-trash-can"></i></button>
                    </td>
                `;
                tripsList.appendChild(tr);
            });
        }
    } catch (err) {
        trainsList.innerHTML = `<tr><td colspan="4" class="no-data-cell" style="color:var(--accent-red)">Error: ${err.message}</td></tr>`;
        tripsList.innerHTML = `<tr><td colspan="4" class="no-data-cell" style="color:var(--accent-red)">Error: ${err.message}</td></tr>`;
    }
}

async function removeUserFollowedTrain(trainId) {
    const confirmMsg = state.language === 'ar'
        ? 'هل أنت متأكد من إلغاء متابعة هذا القطار؟'
        : 'Are you sure you want to remove this train following?';
    if (!confirm(confirmMsg)) return;
    
    try {
        await apiFetch(`/api/admin/users/${currentFollowingsUserId}/following/trains/${trainId}`, { method: 'DELETE' });
        await loadUserFollowings();
        if (state.activeTab === 'trains') loadTrains();
    } catch (err) {
        alert(`Error: ${err.message}`);
    }
}

async function removeUserFollowedTrip(tripId) {
    const confirmMsg = state.language === 'ar'
        ? 'هل أنت متأكد من إلغاء متابعة هذه الرحلة؟'
        : 'Are you sure you want to remove this trip following?';
    if (!confirm(confirmMsg)) return;
    
    try {
        await apiFetch(`/api/admin/users/${currentFollowingsUserId}/following/trips/${tripId}`, { method: 'DELETE' });
        await loadUserFollowings();
        if (state.activeTab === 'trips') loadTrips();
    } catch (err) {
        alert(`Error: ${err.message}`);
    }
}

async function openTrainFollowersModal(trainId, trainNumber) {
    currentTrainFollowersTrainId = trainId;
    document.getElementById('train-followers-title').innerText = `${state.language === 'ar' ? 'متابعي القطار' : 'Followers of Train'} ${trainNumber}`;
    document.getElementById('train-followers-modal').classList.remove('hidden');
    await loadTrainFollowers();
}

function closeTrainFollowersModal() {
    document.getElementById('train-followers-modal').classList.add('hidden');
    currentTrainFollowersTrainId = null;
}

async function loadTrainFollowers() {
    if (!currentTrainFollowersTrainId) return;
    const list = document.getElementById('train-followers-list');
    list.innerHTML = `<tr><td colspan="4" class="loading-cell">${state.language === 'ar' ? 'جاري التحميل...' : 'Loading followers...'}</td></tr>`;
    
    try {
        const followers = await apiFetch(`/api/admin/trains/${currentTrainFollowersTrainId}/followers`);
        list.innerHTML = '';
        
        if (followers.length === 0) {
            list.innerHTML = `<tr><td colspan="4" class="no-data-cell">${state.language === 'ar' ? 'لا يوجد متابعون' : 'No followers found.'}</td></tr>`;
            document.getElementById('btn-remove-all-train-followers').style.display = 'none';
        } else {
            document.getElementById('btn-remove-all-train-followers').style.display = 'inline-block';
            followers.forEach(user => {
                const tr = document.createElement('tr');
                tr.innerHTML = `
                    <td style="font-weight: 600; color: white;">${user.displayName}</td>
                    <td><code>${user.email}</code></td>
                    <td>${formatDaysOfWeek(user.daysOfWeek)}</td>
                    <td class="actions-column">
                        <button class="action-btn delete" onclick="removeTrainFollower('${user.userId}')" title="${state.language === 'ar' ? 'إزالة' : 'Remove'}"><i class="fa-solid fa-trash-can"></i></button>
                    </td>
                `;
                list.appendChild(tr);
            });
        }
    } catch (err) {
        list.innerHTML = `<tr><td colspan="4" class="no-data-cell" style="color:var(--accent-red)">Error: ${err.message}</td></tr>`;
    }
}

async function removeTrainFollower(userId) {
    const confirmMsg = state.language === 'ar'
        ? 'هل أنت متأكد من إزالة هذا المتابع؟'
        : 'Are you sure you want to remove this follower?';
    if (!confirm(confirmMsg)) return;
    
    try {
        await apiFetch(`/api/admin/trains/${currentTrainFollowersTrainId}/followers/${userId}`, { method: 'DELETE' });
        await loadTrainFollowers();
        if (state.activeTab === 'trains') loadTrains();
    } catch (err) {
        alert(`Error: ${err.message}`);
    }
}

async function removeAllTrainFollowers() {
    const confirmMsg = state.language === 'ar'
        ? 'هل أنت متأكد من إزالة جميع المتابعين لهذا القطار؟'
        : 'Are you sure you want to remove ALL followers for this train?';
    if (!confirm(confirmMsg)) return;
    
    try {
        await apiFetch(`/api/admin/trains/${currentTrainFollowersTrainId}/followers`, { method: 'DELETE' });
        await loadTrainFollowers();
        if (state.activeTab === 'trains') loadTrains();
    } catch (err) {
        alert(`Error: ${err.message}`);
    }
}

async function openTripFollowersModal(tripId, trainNumber, tripDate) {
    currentTripFollowersTripId = tripId;
    document.getElementById('trip-followers-title').innerText = `${state.language === 'ar' ? 'متابعي رحلة' : 'Followers of Trip'} ${trainNumber} (${tripDate})`;
    document.getElementById('trip-followers-modal').classList.remove('hidden');
    await loadTripFollowers();
}

function closeTripFollowersModal() {
    document.getElementById('trip-followers-modal').classList.add('hidden');
    currentTripFollowersTripId = null;
}

async function loadTripFollowers() {
    if (!currentTripFollowersTripId) return;
    const list = document.getElementById('trip-followers-list');
    list.innerHTML = `<tr><td colspan="5" class="loading-cell">${state.language === 'ar' ? 'جاري التحميل...' : 'Loading followers...'}</td></tr>`;
    
    try {
        const followers = await apiFetch(`/api/admin/trips/${currentTripFollowersTripId}/followers`);
        list.innerHTML = '';
        
        if (followers.length === 0) {
            list.innerHTML = `<tr><td colspan="5" class="no-data-cell">${state.language === 'ar' ? 'لا يوجد متابعون' : 'No followers found.'}</td></tr>`;
            document.getElementById('btn-remove-all-trip-followers').style.display = 'none';
        } else {
            document.getElementById('btn-remove-all-trip-followers').style.display = 'inline-block';
            followers.forEach(user => {
                const tr = document.createElement('tr');
                const dateStr = user.followedAt ? new Date(user.followedAt).toLocaleString() : '-';
                tr.innerHTML = `
                    <td style="font-weight: 600; color: white;">${user.displayName}</td>
                    <td><code>${user.email}</code></td>
                    <td>${dateStr}</td>
                    <td><span class="status-pill arrived">${user.personalStatus}</span></td>
                    <td class="actions-column">
                        <button class="action-btn delete" onclick="removeTripFollower('${user.userId}')" title="${state.language === 'ar' ? 'إزالة' : 'Remove'}"><i class="fa-solid fa-trash-can"></i></button>
                    </td>
                `;
                list.appendChild(tr);
            });
        }
    } catch (err) {
        list.innerHTML = `<tr><td colspan="5" class="no-data-cell" style="color:var(--accent-red)">Error: ${err.message}</td></tr>`;
    }
}

async function removeTripFollower(userId) {
    const confirmMsg = state.language === 'ar'
        ? 'هل أنت متأكد من إزالة هذا المتابع؟'
        : 'Are you sure you want to remove this follower?';
    if (!confirm(confirmMsg)) return;
    
    try {
        await apiFetch(`/api/admin/trips/${currentTripFollowersTripId}/followers/${userId}`, { method: 'DELETE' });
        await loadTripFollowers();
        if (state.activeTab === 'trips') loadTrips();
    } catch (err) {
        alert(`Error: ${err.message}`);
    }
}

async function removeAllTripFollowers() {
    const confirmMsg = state.language === 'ar'
        ? 'هل أنت متأكد من إزالة جميع المتابعين لهذه الرحلة؟'
        : 'Are you sure you want to remove ALL followers for this trip?';
    if (!confirm(confirmMsg)) return;
    
    try {
        await apiFetch(`/api/admin/trips/${currentTripFollowersTripId}/followers`, { method: 'DELETE' });
        await loadTripFollowers();
        if (state.activeTab === 'trips') loadTrips();
    } catch (err) {
        alert(`Error: ${err.message}`);
    }
}

// Expose globally
window.openUserFollowingsModal = openUserFollowingsModal;
window.closeUserFollowingsModal = closeUserFollowingsModal;
window.switchFollowingTab = switchFollowingTab;
window.loadUserFollowings = loadUserFollowings;
window.removeUserFollowedTrain = removeUserFollowedTrain;
window.removeUserFollowedTrip = removeUserFollowedTrip;

window.openTrainFollowersModal = openTrainFollowersModal;
window.closeTrainFollowersModal = closeTrainFollowersModal;
window.loadTrainFollowers = loadTrainFollowers;
window.removeTrainFollower = removeTrainFollower;
window.removeAllTrainFollowers = removeAllTrainFollowers;

window.openTripFollowersModal = openTripFollowersModal;
window.closeTripFollowersModal = closeTripFollowersModal;
window.loadTripFollowers = loadTripFollowers;
window.removeTripFollower = removeTripFollower;
window.removeAllTripFollowers = removeAllTripFollowers;

window.viewTrain = viewTrain;
window.viewTrip = viewTrip;
window.detailsRemoveTrainFollower = detailsRemoveTrainFollower;
window.detailsRemoveAllTrainFollowers = detailsRemoveAllTrainFollowers;
window.detailsRemoveTripFollower = detailsRemoveTripFollower;
window.detailsRemoveAllTripFollowers = detailsRemoveAllTripFollowers;

window.setTrainTripsFilter = setTrainTripsFilter;
window.changeTrainTripsPage = changeTrainTripsPage;
window.setTrainFollowersSearch = setTrainFollowersSearch;
window.changeTrainFollowersPage = changeTrainFollowersPage;
window.setTripFollowersSearch = setTripFollowersSearch;
window.changeTripFollowersPage = changeTripFollowersPage;

// ==========================================================================
// 🔔 NOTIFICATIONS SYSTEM & DROPDOWN EVENT HANDLERS
// ==========================================================================
async function fetchNotifications() {
    if (!state.token) return;
    try {
        const notifications = await apiFetch('/api/trips/notifications');
        state.notifications = notifications || [];
        renderNotifications();
    } catch (err) {
        console.error('Failed to fetch notifications:', err);
    }
}

function renderNotifications() {
    const listEl = document.getElementById('notifications-list');
    const badgeEl = document.getElementById('notification-badge');
    if (!listEl) return;

    const unreadCount = state.notifications.filter(n => !n.isRead).length;

    // Show/hide unread badge
    if (unreadCount > 0) {
        if (badgeEl) badgeEl.classList.remove('hidden');
    } else {
        if (badgeEl) badgeEl.classList.add('hidden');
    }

    listEl.innerHTML = '';

    if (state.notifications.length === 0) {
        listEl.innerHTML = `<p class="loading-text" style="text-align: center; color: var(--text-muted); font-size: 12px; padding: 20px 0;" data-i18n="no_notifications">${t('no_notifications')}</p>`;
        return;
    }

    state.notifications.forEach(n => {
        const timeStr = new Date(n.createdAt).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
        const item = document.createElement('div');
        item.className = 'notif-dropdown-item';
        item.style.padding = '10px 12px';
        item.style.borderRadius = '8px';
        item.style.background = n.isRead ? 'transparent' : 'rgba(168, 85, 247, 0.05)';
        item.style.borderLeft = n.isRead ? 'none' : '3px solid var(--accent-purple)';
        item.style.cursor = 'pointer';
        item.style.transition = 'background 0.2s ease';
        item.style.display = 'flex';
        item.style.flexDirection = 'column';
        item.style.gap = '4px';

        item.addEventListener('mouseenter', () => { item.style.background = 'rgba(255, 255, 255, 0.03)'; });
        item.addEventListener('mouseleave', () => { item.style.background = n.isRead ? 'transparent' : 'rgba(168, 85, 247, 0.05)'; });

        item.innerHTML = `
            <span class="notif-item-msg" style="font-size: 12px; color: ${n.isRead ? 'var(--text-secondary)' : 'var(--text-primary)'}; font-weight: ${n.isRead ? '400' : '600'};">${n.message}</span>
            <span class="notif-item-time" style="font-size: 10px; color: var(--text-muted);">${timeStr}</span>
        `;

        item.addEventListener('click', async (e) => {
            e.stopPropagation();
            try {
                if (!n.isRead) {
                    await apiFetch(`/api/trips/notifications/${n.id}/read`, { method: 'PUT' });
                    n.isRead = true;
                    renderNotifications();
                }
                const notifDropdown = document.getElementById('notifications-dropdown');
                if (notifDropdown) notifDropdown.classList.add('hidden');
                if (n.link) {
                    let tabName = n.link.replace(/^\//, '');
                    if (tabName === 'lost-found') tabName = 'lostfound';
                    if (tabName) {
                        switchTab(tabName);
                    }
                }
            } catch (err) {
                console.error('Failed to process notification click:', err);
            }
        });

        listEl.appendChild(item);
    });
}

async function markAllNotificationsAsRead() {
    try {
        await apiFetch('/api/trips/notifications/read', { method: 'PUT' });
        state.notifications.forEach(n => n.isRead = true);
        renderNotifications();
    } catch (err) {
        console.error('Failed to mark all as read:', err);
    }
}

function setupDropdownListeners() {
    const profileTrigger = document.getElementById('profile-trigger');
    const profileDropdown = document.getElementById('profile-dropdown');
    const notifTrigger = document.getElementById('notifications-toggle-btn');
    const notifDropdown = document.getElementById('notifications-dropdown');
    const markAllReadBtn = document.getElementById('mark-all-read-btn');

    if (profileTrigger && profileDropdown) {
        profileTrigger.addEventListener('click', (e) => {
            e.stopPropagation();
            profileDropdown.classList.toggle('hidden');
            if (notifDropdown) notifDropdown.classList.add('hidden');
        });
    }

    if (notifTrigger && notifDropdown) {
        notifTrigger.addEventListener('click', (e) => {
            e.stopPropagation();
            notifDropdown.classList.toggle('hidden');
            if (profileDropdown) profileDropdown.classList.add('hidden');
            if (!notifDropdown.classList.contains('hidden')) {
                fetchNotifications(); // Refresh list on open
            }
        });
    }

    if (markAllReadBtn) {
        markAllReadBtn.addEventListener('click', (e) => {
            e.stopPropagation();
            markAllNotificationsAsRead();
        });
    }

    // Click outside to close dropdowns
    document.addEventListener('click', (e) => {
        if (profileDropdown && !profileDropdown.classList.contains('hidden') && !e.target.closest('.user-menu-container')) {
            profileDropdown.classList.add('hidden');
        }
        if (notifDropdown && !notifDropdown.classList.contains('hidden') && !e.target.closest('.notifications-menu-container')) {
            notifDropdown.classList.add('hidden');
        }
    });
}

// Expose notifications API globally
window.fetchNotifications = fetchNotifications;
window.markAllNotificationsAsRead = markAllNotificationsAsRead;

// ==========================================================================
// 🗺️ RAILWAY PATHS CRUD LOGIC
// ==========================================================================
async function loadRailwayPaths() {
    const tableBody = document.querySelector('#railway-paths-list');
    tableBody.innerHTML = `<tr><td colspan="5" class="loading-cell">Loading railway paths...</td></tr>`;
    
    try {
        if (state.stops.length === 0) {
            state.stops = await apiFetch('/api/admin/stops');
        }
        
        const paths = await apiFetch('/api/admin/railway-paths');
        state.railwayPaths = paths;
        renderRailwayPathsTable(paths);
    } catch (err) {
        tableBody.innerHTML = `<tr><td colspan="5" class="no-data-cell" style="color:var(--accent-red)">Error loading railway paths: ${err.message}</td></tr>`;
    }
}

function renderRailwayPathsTable(paths) {
    const tableBody = document.querySelector('#railway-paths-list');
    tableBody.innerHTML = '';

    if (paths.length === 0) {
        tableBody.innerHTML = `<tr><td colspan="5" class="no-data-cell">No railway paths defined.</td></tr>`;
        return;
    }

    paths.forEach(path => {
        const tr = document.createElement('tr');
        
        const startStopName = state.language === 'ar' ? (path.startStationNameAr || path.startStationNameEn) : (path.startStationNameEn || path.startStationNameAr);
        const endStopName = state.language === 'ar' ? (path.endStationNameAr || path.endStationNameEn) : (path.endStationNameEn || path.endStationNameAr);
        
        const pointsCount = path.routePath ? path.routePath.length : 0;
        const dateStr = new Date(path.createdAt).toLocaleString();

        tr.innerHTML = `
            <td style="font-weight: 600; color: white;">${startStopName}</td>
            <td style="font-weight: 600; color: white;">${endStopName}</td>
            <td>${pointsCount} points</td>
            <td style="color: var(--text-secondary); font-size: 13px;">${dateStr}</td>
            <td class="actions-column">
                <button class="action-btn view" onclick="previewRailwayPath('${path.id}')" title="Preview on Map"><i class="fa-solid fa-map"></i></button>
                <button class="action-btn view" style="background-color: var(--accent-orange);" onclick="downloadRailwayPathGeoJson('${path.id}')" title="Download GeoJSON"><i class="fa-solid fa-download"></i></button>
                <button class="action-btn edit" onclick="editRailwayPath('${path.id}')" title="Edit Geometry"><i class="fa-solid fa-pencil"></i></button>
                <button class="action-btn delete" onclick="deleteRailwayPath('${path.id}')" title="Delete Path"><i class="fa-solid fa-trash"></i></button>
            </td>
        `;
        tableBody.appendChild(tr);
    });
}

// Search filter for railway paths
const searchEl = document.getElementById('railway-path-search');
if (searchEl) {
    searchEl.addEventListener('input', (e) => {
        const query = e.target.value.toLowerCase();
        const rows = document.querySelectorAll('#railway-paths-table tbody tr');
        rows.forEach(row => {
            if (row.cells.length < 2) return;
            const text = row.innerText.toLowerCase();
            if (text.includes(query)) {
                row.style.display = '';
            } else {
                row.style.display = 'none';
            }
        });
    });
}

async function openRailwayPathModal(path = null) {
    const modal = document.getElementById('railway-path-modal');
    const title = document.getElementById('railway-path-modal-title');
    const errorEl = document.getElementById('railway-path-error');
    
    errorEl.classList.add('hidden');
    document.getElementById('railway-path-form').reset();
    document.getElementById('railway-path-id').value = '';

    const startSelect = document.getElementById('railway-path-start');
    const endSelect = document.getElementById('railway-path-end');
    
    startSelect.disabled = false;
    endSelect.disabled = false;
    
    // Populate station dropdowns
    if (state.stops.length === 0) {
        state.stops = await apiFetch('/api/admin/stops');
    }
    
    startSelect.innerHTML = '<option value="">-- Select Start Station --</option>';
    endSelect.innerHTML = '<option value="">-- Select End Station --</option>';
    
    state.stops.forEach(stop => {
        const stopName = state.language === 'ar' ? stop.nameAr : stop.nameEn;
        const optText = `${stopName} (${stop.code})`;
        
        const optStart = document.createElement('option');
        optStart.value = stop.id;
        optStart.textContent = optText;
        startSelect.appendChild(optStart);

        const optEnd = document.createElement('option');
        optEnd.value = stop.id;
        optEnd.textContent = optText;
        endSelect.appendChild(optEnd);
    });

    const submitBtn = document.getElementById('railway-path-submit-btn');

    if (path) {
        title.textContent = 'Edit Railway Path Geometry';
        submitBtn.textContent = 'Update Geometry';
        
        document.getElementById('railway-path-id').value = path.id;
        startSelect.value = path.startStationId;
        endSelect.value = path.endStationId;
        
        startSelect.disabled = true;
        endSelect.disabled = true;

        // Convert coordinates back to GeoJSON LineString format [Lng, Lat]
        const coordinates = path.routePath.map(coords => [coords[1], coords[0]]);
        const geoJsonObj = {
            type: 'Feature',
            properties: {
                start: path.startStationNameEn,
                end: path.endStationNameEn
            },
            geometry: {
                type: 'LineString',
                coordinates: coordinates
            }
        };
        document.getElementById('railway-path-geojson').value = JSON.stringify(geoJsonObj, null, 2);
    } else {
        title.textContent = 'Define New Railway Path';
        submitBtn.textContent = 'Create Path';
    }

    modal.classList.remove('hidden');
}

function closeRailwayPathModal() {
    document.getElementById('railway-path-modal').classList.add('hidden');
}

function handleRailwayPathFileUpload(e) {
    const file = e.target.files[0];
    if (!file) return;

    const errorEl = document.getElementById('railway-path-error');
    const errorTextEl = document.getElementById('railway-path-error-text');
    errorEl.classList.add('hidden');

    const reader = new FileReader();
    reader.onload = (event) => {
        try {
            const text = event.target.result;
            // Parse to validate JSON structure
            JSON.parse(text); 
            document.getElementById('railway-path-geojson').value = text;
        } catch (err) {
            errorTextEl.textContent = 'Uploaded file is not in valid JSON/GeoJSON format.';
            errorEl.classList.remove('hidden');
        }
    };
    reader.readAsText(file);
    e.target.value = null; // reset
}

const pathForm = document.getElementById('railway-path-form');
if (pathForm) {
    pathForm.addEventListener('submit', async (e) => {
        e.preventDefault();
        const id = document.getElementById('railway-path-id').value;
        const startStationId = document.getElementById('railway-path-start').value;
        const endStationId = document.getElementById('railway-path-end').value;
        const geoJsonText = document.getElementById('railway-path-geojson').value;

        const errorEl = document.getElementById('railway-path-error');
        const errorTextEl = document.getElementById('railway-path-error-text');
        errorEl.classList.add('hidden');

        // Validation
        if (!startStationId || !endStationId) {
            errorTextEl.textContent = 'Please select both Start and End Stations.';
            errorEl.classList.remove('hidden');
            return;
        }

        if (startStationId === endStationId) {
            errorTextEl.textContent = 'Start Station and End Station cannot be identical.';
            errorEl.classList.remove('hidden');
            return;
        }

        if (!geoJsonText.trim()) {
            errorTextEl.textContent = 'Please provide GeoJSON path geometry.';
            errorEl.classList.remove('hidden');
            return;
        }

        try {
            const parsed = JSON.parse(geoJsonText);
            
            let geometry = parsed;
            if (parsed.type === 'Feature') {
                geometry = parsed.geometry;
            } else if (parsed.type === 'FeatureCollection') {
                const feature = parsed.features?.[0];
                if (feature) {
                    geometry = feature.geometry;
                }
            }

            if (!geometry || geometry.type !== 'LineString' || !Array.isArray(geometry.coordinates)) {
                errorTextEl.textContent = 'GeoJSON must define a valid LineString geometry.';
                errorEl.classList.remove('hidden');
                return;
            }

            if (geometry.coordinates.length < 2) {
                errorTextEl.textContent = 'A railway path LineString must have at least 2 coordinate points.';
                errorEl.classList.remove('hidden');
                return;
            }
        } catch (err) {
            errorTextEl.textContent = 'Invalid JSON format: ' + err.message;
            errorEl.classList.remove('hidden');
            return;
        }

        // Check for duplicates (only for creation)
        if (!id && state.railwayPaths) {
            const duplicate = state.railwayPaths.some(p => 
                (p.startStationId === startStationId && p.endStationId === endStationId) ||
                (p.startStationId === endStationId && p.endStationId === startStationId)
            );
            if (duplicate) {
                errorTextEl.textContent = 'A railway path already exists between these stations. Please edit the existing path instead.';
                errorEl.classList.remove('hidden');
                return;
            }
        }

        try {
            if (id) {
                await apiFetch(`/api/admin/railway-paths/${id}`, {
                    method: 'PUT',
                    body: JSON.stringify({ geoJsonContent: geoJsonText })
                });
            } else {
                await apiFetch('/api/admin/railway-paths', {
                    method: 'POST',
                    body: JSON.stringify({
                        startStationId,
                        endStationId,
                        geoJsonContent: geoJsonText
                    })
                });
            }
            closeRailwayPathModal();
            loadRailwayPaths();
        } catch (err) {
            errorTextEl.textContent = err.message || 'Operation failed.';
            errorEl.classList.remove('hidden');
        }
    });
}

async function editRailwayPath(id) {
    if (!state.railwayPaths) return;
    const path = state.railwayPaths.find(p => p.id === id);
    if (path) openRailwayPathModal(path);
}

async function deleteRailwayPath(id) {
    if (!confirm('Are you sure you want to delete this railway path? Associated stations will not be deleted.')) return;
    try {
        await apiFetch(`/api/admin/railway-paths/${id}`, { method: 'DELETE' });
        loadRailwayPaths();
    } catch (err) {
        alert(`Error deleting railway path: ${err.message}`);
    }
}

let previewMapInstance = null;
let previewPolyline = null;
let previewMarkers = [];

async function previewRailwayPath(id) {
    if (!state.railwayPaths) return;
    const path = state.railwayPaths.find(p => p.id === id);
    if (!path) return;

    state.currentPreviewPath = path;

    const modal = document.getElementById('railway-path-preview-modal');
    const title = document.getElementById('railway-path-preview-title');
    const subtitle = document.getElementById('railway-path-preview-subtitle');
    const pointsCountEl = document.getElementById('railway-path-preview-points-count');

    const startName = state.language === 'ar' ? (path.startStationNameAr || path.startStationNameEn) : (path.startStationNameEn || path.startStationNameAr);
    const endName = state.language === 'ar' ? (path.endStationNameAr || path.endStationNameEn) : (path.endStationNameEn || path.endStationNameAr);

    title.textContent = 'Railway Path Route Preview';
    subtitle.textContent = `${startName} ⇄ ${endName}`;
    
    const coordinatesCount = path.routePath ? path.routePath.length : 0;
    pointsCountEl.textContent = `Total Points: ${coordinatesCount}`;

    modal.classList.remove('hidden');

    // Wait for DOM layout
    setTimeout(() => {
        const container = document.getElementById('railway-path-preview-map');
        if (!container) return;

        if (typeof L === 'undefined') {
            container.innerHTML = `
                <div style="display:flex; flex-direction:column; align-items:center; justify-content:center; height:100%; color:var(--text-secondary); font-size:13px; padding:20px; text-align:center; background:rgba(255,255,255,0.01);">
                    <i class="fa-solid fa-triangle-exclamation" style="margin-bottom:8px; font-size:24px; color:var(--accent-orange);"></i>
                    <span>Map features are currently unavailable.</span>
                </div>
            `;
            return;
        }

        if (!previewMapInstance) {
            previewMapInstance = L.map('railway-path-preview-map', {
                zoomControl: true,
                scrollWheelZoom: true
            }).setView([30.0444, 31.2357], 10);

            // Light map tile base layer as requested
            L.tileLayer('https://{s}.basemaps.cartocdn.com/light_all/{z}/{x}/{y}{r}.png', {
                attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors &copy; <a href="https://carto.com/attributions">CARTO</a>',
                maxZoom: 20
            }).addTo(previewMapInstance);
        }

        const map = previewMapInstance;
        map.invalidateSize();

        // Clear previous layers
        if (previewPolyline) {
            previewPolyline.remove();
            previewPolyline = null;
        }
        previewMarkers.forEach(m => m.remove());
        previewMarkers = [];

        // Plot Polyline
        const coordinates = path.routePath; // [Lat, Lng] from API
        if (coordinates && coordinates.length > 0) {
            previewPolyline = L.polyline(coordinates, {
                color: '#3b82f6', // Elegant blue track line
                weight: 6,
                opacity: 0.85,
                lineCap: 'round',
                lineJoin: 'round'
            }).addTo(map);

            map.fitBounds(previewPolyline.getBounds(), { padding: [40, 40] });
        }

        // Start station marker
        if (state.stops.length === 0) return;
        const startStop = state.stops.find(s => s.id === path.startStationId);
        if (startStop) {
            const startIcon = L.divIcon({
                className: 'custom-station-marker start',
                html: `<div style="background-color: #10b981; width: 14px; height: 14px; border-radius: 50%; border: 2px solid white; box-shadow: 0 0 8px rgba(0,0,0,0.3)"></div>`,
                iconSize: [14, 14],
                iconAnchor: [7, 7]
            });
            const m = L.marker([startStop.latitude, startStop.longitude], { icon: startIcon })
                .addTo(map)
                .bindPopup(`<b>Start Station:</b> ${startName}`);
            previewMarkers.push(m);
        }

        // End station marker
        const endStop = state.stops.find(s => s.id === path.endStationId);
        if (endStop) {
            const endIcon = L.divIcon({
                className: 'custom-station-marker end',
                html: `<div style="background-color: #ef4444; width: 14px; height: 14px; border-radius: 50%; border: 2px solid white; box-shadow: 0 0 8px rgba(0,0,0,0.3)"></div>`,
                iconSize: [14, 14],
                iconAnchor: [7, 7]
            });
            const m = L.marker([endStop.latitude, endStop.longitude], { icon: endIcon })
                .addTo(map)
                .bindPopup(`<b>End Station:</b> ${endName}`);
            previewMarkers.push(m);
        }
    }, 150);
}

function closeRailwayPathPreviewModal() {
    document.getElementById('railway-path-preview-modal').classList.add('hidden');
    // Clean up map instance when closed
    if (previewMapInstance) {
        previewMapInstance.remove();
        previewMapInstance = null;
    }
    previewPolyline = null;
    previewMarkers = [];
    state.currentPreviewPath = null;
}

function downloadRailwayPathGeoJson(id) {
    if (!state.railwayPaths) return;
    const path = state.railwayPaths.find(p => p.id === id);
    if (!path) return;
    
    const coordinates = path.routePath.map(coords => [coords[1], coords[0]]);
    const geoJsonObj = {
        type: 'Feature',
        properties: {
            startStationNameEn: path.startStationNameEn,
            startStationNameAr: path.startStationNameAr,
            endStationNameEn: path.endStationNameEn,
            endStationNameAr: path.endStationNameAr
        },
        geometry: {
            type: 'LineString',
            coordinates: coordinates
        }
    };
    
    const blob = new Blob([JSON.stringify(geoJsonObj, null, 2)], { type: 'application/json' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    const startName = (path.startStationNameEn || 'Station').replace(/\s+/g, '_');
    const endName = (path.endStationNameEn || 'Station').replace(/\s+/g, '_');
    a.download = `${startName}_to_${endName}.geojson`;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    URL.revokeObjectURL(url);
}

function downloadCurrentPreviewPathGeoJson() {
    if (state.currentPreviewPath) {
        downloadRailwayPathGeoJson(state.currentPreviewPath.id);
    }
}

async function loadLiveUpdatesModeration() {
    const pendingContainer = document.querySelector('#pending-updates-list');
    const removalContainer = document.querySelector('#removal-requests-list');
    
    pendingContainer.innerHTML = `<div class="loading-cell">Loading pending updates...</div>`;
    removalContainer.innerHTML = `<div class="loading-cell">Loading removal requests...</div>`;
    
    try {
        const [pendingRes, removalRes] = await Promise.all([
            apiFetch('/api/admin/trips/updates/pending'),
            apiFetch('/api/admin/trips/updates/removal-requests')
        ]);
        
        state.pendingUpdates = pendingRes;
        state.removalRequests = removalRes;
        
        renderPendingUpdates(pendingRes);
        renderRemovalRequests(removalRes);
    } catch (err) {
        pendingContainer.innerHTML = `<div style="padding: 10px; color: var(--accent-red)">Error: ${err.message}</div>`;
        removalContainer.innerHTML = `<div style="padding: 10px; color: var(--accent-red)">Error: ${err.message}</div>`;
    }
}

function renderPendingUpdates(updates) {
    const container = document.querySelector('#pending-updates-list');
    container.innerHTML = '';
    
    if (updates.length === 0) {
        container.innerHTML = `<p style="color: var(--text-secondary); font-size: 13px;">${t('noPendingUpdates')}</p>`;
        return;
    }
    
    updates.forEach(upd => {
        const card = document.createElement('div');
        card.style = 'padding: 16px; border-radius: 10px; border: 1px solid var(--border-color); background: rgba(120,120,120,0.01); display: flex; justify-content: space-between; align-items: center; flex-wrap: wrap; gap: 12px; margin-bottom: 12px;';
        
        let badgesHtml = '';
        if (upd.statusTag) badgesHtml += `<span class="badge badge-info" style="font-size: 10px; margin-right: 6px;">${upd.statusTag}</span>`;
        if (upd.crowdState) badgesHtml += `<span class="badge badge-admin" style="font-size: 10px; background-color: var(--accent-orange); color: white;">${upd.crowdState}</span>`;
        
        card.innerHTML = `
            <div style="flex-grow: 1;">
                <div style="font-weight: 700; color: white; font-size: 14px;">
                    Train #${upd.trainNumber} (${upd.tripDate}) - By: ${upd.authorName}
                </div>
                <p style="font-size: 13px; color: var(--text-secondary); margin: 6px 0 0 0;">
                    ${upd.content}
                </p>
                <div style="margin-top: 8px;">
                    ${badgesHtml}
                </div>
            </div>
            <div style="display: flex; gap: 8px;">
                <button onclick="rejectPendingUpdate('${upd.id}')" class="btn btn-secondary" style="padding: 6px 12px; font-size: 12px; border-color: var(--accent-red); color: var(--accent-red); background: transparent; height: auto;">
                    ${t('reject')}
                </button>
                <button onclick="approvePendingUpdate('${upd.id}')" class="btn btn-primary" style="padding: 6px 12px; font-size: 12px; background: var(--accent-green); border-color: var(--accent-green); height: auto; color: white;">
                    ${t('approve')}
                </button>
            </div>
        `;
        container.appendChild(card);
    });
}

function renderRemovalRequests(updates) {
    const container = document.querySelector('#removal-requests-list');
    container.innerHTML = '';
    
    if (updates.length === 0) {
        container.innerHTML = `<p style="color: var(--text-secondary); font-size: 13px;">${t('noRemovalRequests')}</p>`;
        return;
    }
    
    updates.forEach(upd => {
        const card = document.createElement('div');
        card.style = 'padding: 16px; border-radius: 10px; border: 1px solid rgba(239,68,68,0.2); background: rgba(239,68,68,0.02); display: flex; justify-content: space-between; align-items: center; flex-wrap: wrap; gap: 12px; margin-bottom: 12px;';
        
        let badgesHtml = '';
        if (upd.statusTag) badgesHtml += `<span class="badge badge-info" style="font-size: 10px; margin-right: 6px;">${upd.statusTag}</span>`;
        if (upd.crowdState) badgesHtml += `<span class="badge badge-admin" style="font-size: 10px; background-color: var(--accent-orange); color: white;">${upd.crowdState}</span>`;
        
        card.innerHTML = `
            <div style="flex-grow: 1;">
                <div style="font-weight: 700; color: white; font-size: 14px;">
                    Train #${upd.trainNumber} (${upd.tripDate}) - By: ${upd.authorName}
                </div>
                <p style="font-size: 13px; color: var(--text-secondary); margin: 6px 0 0 0;">
                    ${upd.content}
                </p>
                <div style="margin-top: 8px;">
                    ${badgesHtml}
                </div>
            </div>
            <div style="display: flex; gap: 8px;">
                <button onclick="denyRemovalRequest('${upd.id}')" class="btn btn-secondary" style="padding: 6px 12px; font-size: 12px; height: auto;">
                    ${t('denyRemoval')}
                </button>
                <button onclick="confirmRemovalRequest('${upd.id}')" class="btn btn-primary" style="padding: 6px 12px; font-size: 12px; background: var(--accent-red); border-color: var(--accent-red); height: auto; color: white;">
                    ${t('confirmRemoval')}
                </button>
            </div>
        `;
        container.appendChild(card);
    });
}

async function approvePendingUpdate(id) {
    try {
        await apiFetch(`/api/admin/trips/updates/${id}/approve`, { method: 'PUT' });
        alert(t('approve_update_success'));
        loadLiveUpdatesModeration();
    } catch (err) {
        alert('Error approving update: ' + err.message);
    }
}

async function rejectPendingUpdate(id) {
    if (!confirm(t('confirm_delete_update'))) return;
    try {
        await apiFetch(`/api/admin/trips/updates/${id}`, { method: 'DELETE' });
        alert(t('delete_update_success'));
        loadLiveUpdatesModeration();
    } catch (err) {
        alert('Error deleting update: ' + err.message);
    }
}

async function confirmRemovalRequest(id) {
    if (!confirm(t('confirm_delete_update'))) return;
    try {
        await apiFetch(`/api/admin/trips/updates/${id}`, { method: 'DELETE' });
        alert(t('delete_update_success'));
        loadLiveUpdatesModeration();
    } catch (err) {
        alert('Error confirming removal: ' + err.message);
    }
}

async function denyRemovalRequest(id) {
    try {
        await apiFetch(`/api/admin/trips/updates/${id}/deny-removal`, { method: 'POST' });
        alert(t('deny_removal_success'));
        loadLiveUpdatesModeration();
    } catch (err) {
        alert('Error denying removal: ' + err.message);
    }
}

window.loadRailwayPaths = loadRailwayPaths;
window.openRailwayPathModal = openRailwayPathModal;
window.closeRailwayPathModal = closeRailwayPathModal;
window.handleRailwayPathFileUpload = handleRailwayPathFileUpload;
window.editRailwayPath = editRailwayPath;
window.deleteRailwayPath = deleteRailwayPath;
window.previewRailwayPath = previewRailwayPath;
window.closeRailwayPathPreviewModal = closeRailwayPathPreviewModal;
window.downloadRailwayPathGeoJson = downloadRailwayPathGeoJson;
window.downloadCurrentPreviewPathGeoJson = downloadCurrentPreviewPathGeoJson;
window.loadLiveUpdatesModeration = loadLiveUpdatesModeration;
window.approvePendingUpdate = approvePendingUpdate;
window.rejectPendingUpdate = rejectPendingUpdate;
window.confirmRemovalRequest = confirmRemovalRequest;
window.denyRemovalRequest = denyRemovalRequest;


