const emptyGuid = "00000000-0000-0000-0000-000000000000";
const multiSelectLocalLang = {
    selectAll: "انتخاب همه",
    selectNone: "حذف همه",
    reset: "بازنشانی",
    search: "جستجو ...",
    nothingSelected: "موردی انتخاب نشده است"
}
const userType = {
    Admin: 0,
    User: 1,
    Labels: [
        { id: 0, title: "Admin" },
        { id: 1, title: "User" },
    ]
}
const priority = {
    High: 0,
    Medium: 1,
    Low: 2,
    Labels: [
        { id: 0, title: "High" },
        { id: 1, title: "Medium" },
        { id: 2, title: "Low" }
    ]
}
const ticketStatus = {
    Open: 0,
    Seen: 1,
    Close: 2,
    Labels: [
        { id: 0, title: "Open" },
        { id: 1, title: "Seen" },
        { id: 2, title: "Closed" }
    ]
}
const skillValue = [
    1,
    2,
    3,
    4,
    5,
];
const fileType = {
    Video: 0,
    Pdf: 1,
    Image: 2,
    Labels: [
        { id: 0, title: "Video" },
        { id: 1, title: "Pdf" },
        { id: 2, title: "Image" }
    ]
}
const jobType = [
    "LinkedIn",
    "Xing",
    "Stepstone",
    "Indeed",
    "Link",
];
const suggestType = [
    "Sent",
    "NewQuestions",
    "Rejected",
    "Passed",
    "Interview",
    "ReadyToSend"
];
const applyType = {
    Auto: 0,
    Country: 1,
    English: 2,
    Labels: [
        { id: 0, title: "Auto Apply" },
        { id: 1, title: "Generate Cover Letter as Job's Country" },
        { id: 2, title: "Generate Cover Letter just English" }
    ]
}
const paymentState = {
    Payed: 0,
    NotPayed: 1,
    Labels: [
        { id: 0, title: "Payed" },
        { id: 1, title: "Not Payed" }
    ]
}
const jobTrackerSearch = [
    "All",
    "Rejects",
    "Passed",
    "Interviews",
    "Sent",
];
const inputType = {
    Text: 0,
    Image: 1,
    Video: 2,
    Pdf: 3,
    Link: 4,
    Labels: [
        { id: 0, title: "متن" },
        { id: 1, title: "عکس" },
        { id: 2, title: "ویدیو" },
        { id: 3, title: "پی دی اف" },
        { id: 4, title: "لینک" },
    ]
}