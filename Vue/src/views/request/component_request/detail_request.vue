<script setup>
import { ref, inject, onMounted } from "vue";
import { useToast } from "vue-toastification";
import { encr } from "../../../util/function";
import { VuemojiPicker } from "vuemoji-picker";
import moment from "moment";
import treeuser from "../../../components/user/treeuser.vue";

const toast = useToast();
const axios = inject("axios");
const store = inject("store");
const swal = inject("$swal");
const cryoptojs = inject("cryptojs");
const emitter = inject("emitter");
const basedomainURL = baseURL;
const baseUrlCheck = baseURL;
const config = {
	headers: { Authorization: `Bearer ${store.getters.token}` },
};
const bgColor = ref([
    "#F8E69A",
    "#AFDFCF",
    "#F4B2A3",
    "#9A97EC",
    "#CAE2B0",
    "#8BCFFB",
    "#CCADD7",
]);
const renderColorProgress = (value) => {
    if (value >= 75) {
        return "classOver75";
    }
    else if (value >= 50) {
        return "classOver50";
    }
    else if (value >= 30) {
        return "classOver30";
    }
    else if (value > 0) {
        return "classOver0";
    }
    return "";
};
const groupBy = (xs, key) => {
    return xs.reduce((rv, x) => {
        (rv[x[key]] = rv[x[key]] || []).push(x);
        return rv;
    }, {});
};
function isValidDate(date) {
    //return d instanceof Date && !isNaN(d);
    return (new Date(date) !== "Invalid Date") && !isNaN(new Date(date))
}
const listStatusRequests = ref([
    { id: 0,  text: "Mới lập",    class: "rqlap" },
    { id: 1,  text: "Chờ duyệt",  class: "rqchoduyet" },
    { id: 2,  text: "Chấp thuận", class: "rqchapthuan" },
    { id: -2, text: "Từ chối",    class: "rqtuchoi" },
    { id: -1, text: "Hủy",        class: "rqhuy" },
    { id: 3,  text: "Thu hồi",    class: "rqthuhoi" },
    { id: -3, text: "Xóa",        class: "rqxoa" }
]);
const orderDatas = (dataJob, type) => {
    let resultOrder = dataJob.sort((a, b) => { 
        return a[type] - b[type];
    });
    return resultOrder;
};
const limitData = (dataGet, limit) => {
    return dataGet.slice(0, (limit >= 0 ? limit : 0));
};
const filterFileType = (dataFilter, typeFilter, value) => {
    return dataFilter.filter(x => x[typeFilter] == value);
};
const formatByte = ((bytes, precision) => {
	if (isNaN(parseFloat(bytes)) || !isFinite(bytes)) return '-';
	if (typeof precision === 'undefined') precision = 1;
	let units = ['bytes', 'KB', 'MB', 'GB', 'TB', 'PB'];
	if (typeof bytes === 'string' || bytes instanceof String){
		bytes = parseFloat(bytes);
	}
	let	number = Math.floor(Math.log(bytes) / Math.log(1024));
	return (bytes / Math.pow(1024, Math.floor(number))).toFixed(precision) + ' ' + units[number];
});

const props = defineProps({
    isShow: Boolean,
    id: String,
    key: Number,
    listStatusRequests: Array,
});
const loadData = (rf) => {
    if (rf) {
        loadDetailRequest();
    }
};
const detail_request = ref();
const TimeToDo = ref();
const isClose = ref(false);
const FormDS = ref([]);
const Ftables = ref([]);
const loadDetailRequest = () => {
    axios
    .post(
        baseURL + "/api/request/getData",
        {
            str: encr(
                JSON.stringify({
                    proc: "request_detail_get",
                    par: [
                        { par: "request_id", va: props.id },
                        { par: "user_id", va: store.state.user.user_id },
                    ],
                }),
                SecretKey,
                cryoptojs,
            ).toString(),
        },
        config,
    )
    .then((response) => {
        if (response.data != null && response.data.data != null) {
            var data = JSON.parse(response.data.data);
            if (data.length > 0) {
                detail_request.value = data[0][0];                
                detail_request.value.objStatus = props.listStatusRequests.find(x => x.id == detail_request.value.status_processing);
                detail_request.value.times_processing_max = detail_request.value.times_processing_max || 0;
                // temp fake 1
                detail_request.value.IsViewXL = false; // r.IsViewXL;
                //detail_request.value.Tiendo = 20;
                // end temp fake 1

                if (detail_request.value.is_security) {
                    is_viewSecurityRequest.value = true;
                }
                else {                    
                    // temp fake
                    //is_viewSecurityRequest.value = true; // false;
                }
                
                let today = new Date();
                var d2 = detail_request.value.completed_date ? new Date(detail_request.value.completed_date) : new Date();
                var diff = d2.getTime() - today.getTime();
                var daydiff = diff / (1000 * 60 * 60 * 24);
                var stdate = new Date(detail_request.value.start_date);
                if (stdate == null || stdate > today) {
                    TimeToDo.value = "Chưa bắt đầu";
                }
                else {
                    if (0 < daydiff + 1 && daydiff + 1 < 1) {
                        TimeToDo.value =
                        "<div class='flex format-center font-bold' style='background-color: #fffbd8;color: #6DD230'> Đến hạn hoàn thành </div>";
                        return;
                    }
                    let displayTime = Math.abs(Math.floor(daydiff + 1));
                    TimeToDo.value =
                        daydiff + 1 < 0
                        ? "<div class='flex format-center font-bold' style='background-color: #fffbd8;color: red'> Quá hạn " +
                            displayTime +
                            " ngày</div>"
                        : "<div class='flex format-center font-bold' style='background-color: #fffbd8;color: #6DD230'> Còn " +
                            displayTime +
                            " ngày</div>";
                }
                if (data[1] != null && data[1].length > 0) {
                    FormDS.value = data[1].filter(x => x.is_order_row == null);
                    var fd = data[1].filter(x => x.kieu_truong.toLowerCase() == "radio" && x.value_field.toLowerCase() == "true");
                    if (fd != null && fd.length > 0) {
                        fd.forEach((r) => {
                            detail_request.value.Radio = r.request_formd_id;
                            r.value_field = r.value_field == "true";
                        });
                    }
                    var fd2 = data[1].filter(x => (x.kieu_truong.toLowerCase() == "checkbox" || x.kieu_truong.toLowerCase() == "switch") && x.value_field.toLowerCase() == "true");
                    if (fd2 != null && fd2.length > 0) {
                        fd2.forEach((r) => {
                            r.value_field = r.value_field == "true";
                        });
                    }
                    var fd3 = data[1].filter(x => (x.kieu_truong == "datetime" || x.kieu_truong == "date") && x.value_field != null && isValidDate(x.value_field));
                    if (fd3 != null && fd3.length > 0) {
                        fd3.forEach((r) => {
                            r.value_field = new Date(r.value_field);
                        });
                    }
                    let ftables = [];
                    data[1].filter(x => x.is_type == 3).forEach((r) => {
                        ftables.push([]);
                        var groups = groupBy(data[1].filter(x => x.is_order_row != null && x.is_parent_id == r.request_formd_id), "is_order_row");
                        for (var k in groups) {
                            var fr = [];
                            groups[k].forEach(function (rr) {
                                fr.push({ ...rr });
                            });
                            ftables[ftables.length - 1].push(fr);
                        }
                        Ftables.value = ftables;
                    });
                }
            }
            else {
                detail_request.value = null;
            }
            listFiles();
        }
    })
    .catch((error) => {
        if (error && error.status === 401) {
            swal.fire({
                text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
                confirmButtonText: "OK",
            });
            store.commit("gologout");
        }
    });
};
const formDS_filter = (parentFilter) => {
    return FormDS.value.filter(x => x.is_parent_id == parentFilter);
};

const LisFileAttachRQ = ref([]);
const listFiles = () => {
    axios
        .post(
            baseURL + "/api/request/getData",
            {
                str: encr(
                    JSON.stringify({
                        proc: "request_files",
                        par: [
                            { par: "request_id", va: props.id },
                            { par: "user_id", va: store.state.user.user_id },
                        ],
                    }),
                    SecretKey,
                    cryoptojs,
                ).toString(),
            },
            config,
    )
    .then((response) => {
        if (response.data != null && response.data.data != null && response.data.err != '1') {
            var data = JSON.parse(response.data.data);
            LisFileAttachRQ.value = data[0];
            // fake data
            // LisFileAttachRQ.value = [
            //     { file_name: 'nabila-miah-happy-birthday-gif10774342.gif', file_path: '/Portals/Gif/nabila-miah-happy-birthday-gif10774342.gif', file_type: 'gif', file_size: 135792, is_image: true, is_type: 0, created_date: new Date() },
            //     { file_name: 'Mẫu Excel Phép năm.xlsx', file_path: '/Portals/Mau Excel/Mẫu Excel Phép năm.xlsx', file_type: 'xlsx', file_size: 895792, is_image: false, is_type: 0, created_date: new Date() },
            // ];
            // end fake
        }
    })
    .catch((error) => {
        if (error && error.status === 401) {
            swal.fire({
                text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
                confirmButtonText: "OK",
            });
            store.commit("gologout");
        }
    });
};

const datelines = ref([]);
const listDateline = () => {

};
const Comments = ref([]);
const listComments = () => {
    axios
        .post(
            baseURL + "/api/request/getData",
            {
                str: encr(
                    JSON.stringify({
                        proc: "request_comment_list",
                        par: [
                            { par: "request_id", va: props.id },
                            { par: "user_id", va: store.state.user.user_id },
                        ],
                    }),
                    SecretKey,
                    cryoptojs,
                ).toString(),
            },
            config,
    )
    .then((response) => {
        if (response.data != null && response.data.data != null && response.data.err != '1') {
            var data = JSON.parse(response.data.data);
            if (data.length > 0 && data[0].length > 0) {
                data[0].forEach((el, idx) => {
                    if (el.files != null){
                        el.files = JSON.parse(el.files);
                    }						
                    if (el.sticks != null){
                        el.sticks = JSON.parse(el.sticks);
                    }
                    el.ParentComment = data[0].find(x => el.parent_id === x.request_comment_id);
                });
                Comments.value = data[0];
            }
            else {
                Comments.value = [];
            }
        }
    })
    .catch((error) => {
        if (error && error.status === 401) {
            swal.fire({
                text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
                confirmButtonText: "OK",
            });
            store.commit("gologout");
        }
    });
};

//Bình luận
const panelEmoij1 = ref();
let filecoments = [];
const listFileComment = ref([]);
const comment = ref("");
const comment_zone_main = ref();
let line1 = "";
let line = "";
const showEmoji = (event, check) => {
    if (check == 1) {
        panelEmoij1.value.toggle(event);
    }
};
const Change = (event) => {
  line = event.range.index ? event.range.index : null;
};
const handleEmojiClick = (event) => {  
    comment.value = comment.value.replace("<p>", "").replace("</p>", "");
    line1 = line ? line : comment.value.length;
    let str1 = comment.value.slice(0, line1);
    let str2 = comment.value.slice(line1);
    if (comment.value) {
      comment.value = line1 > 0 ? str1 + event.unicode + str2 : comment.value + event.unicode;
    }
    else {
        comment.value = comment.value + event.unicode;
    }
    comment.value = comment.value.replace("<p>", "").replace("</p>", "");
    comment_zone_main.value.setHTML("<p>" + comment.value + "</p>");
    line1 += 1;  
};
const IsReply = ref(false);

// emote with message
const addEmote = (stick) => {
	let data = { request_id: detail_request.value.request_id, request_comment_id: dataEmote.value.request_comment_id, stick_id: stick.emote_id  };
	axios
		.post(
			baseUrlCheck + "/api/request_comment/Update_Stisk",
			data,
			config
		)
	.then((response) => {
		if (response.data.err !== "1") {
			panelEmote.value.toggle();
			
		}
	})	
	.catch((error) => {
		swal.close();
		if (error.status === 401) {
			swal.fire({
				text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
				confirmButtonText: "OK",
			});
		}
	});
};
const displayStick = ref(false);
const emote_message = ref([]);
const tabStickActive = ref(0);
const getStick = (msg) => {
	axios
    .post(
	  	baseUrlCheck + "api/request/getData",
		{ 
			str: encr(JSON.stringify({
					proc: "request_stick_by_comment",
					par: [
						{ par: "user_id", va: store.getters.user.user_id },
						{ par: "request_comment_id", va: msg.request_comment_id },
					],
				}), SecretKey, cryoptojs
			).toString()
		},
		config
    )
    .then((response) => {
		let data = JSON.parse(response.data.data)[0];
		emote_message.value = data;
		emoteList.value.forEach((st, idx) => {
			st.sticks = emote_message.value.filter(x => x.stick_id === st.emote_id);
		});
		displayStick.value = true;
    })
    .catch((error) => {
		//toast.error("Tải dữ liệu không thành công!");
		//console.log("Error list emotes.");
		if (error && error.status === 401) {
			swal.fire({
				text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
				confirmButtonText: "OK",
			});
			store.commit("gologout");
		}
    });
};
const changeTabStick = (event) => {
	tabStickActive.value = event.index;
}
// emote in chat
const emoteList = ref([]);
const loadEmote = () => {
  axios
    .post(
	  	basedomainURL + "api/request/getData",
		{ 
			str: encr(JSON.stringify({
					proc: "chat_emote_list",
					par: [
						{ par: "user_id", va: store.getters.user.user_id },
					],
				}), SecretKey, cryoptojs
			).toString()
		},
		config
    )
    .then((response) => {
		let data = JSON.parse(response.data.data)[0];
		emoteList.value = data;
    })
    .catch((error) => {
		//toast.error("Tải dữ liệu không thành công!");
		console.log("Error list emotes.");
		if (error && error.status === 401) {
			swal.fire({
				text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
				confirmButtonText: "OK",
			});
			store.commit("gologout");
		}
    });
};
const panelEmote = ref();
const dataEmote = ref({ request_comment_id: null, emote_id: null });
const showEmote = (event, data) => {
	panelEmote.value.toggle(event);
	dataEmote.value.request_comment_id = data.request_comment_id;
};
const checkHover = ref();
const onCheckHover = (item) => {
  checkHover.value = item.emote_id;
};
const hideCheckHover = () => {
  checkHover.value = null;
};
//Thêm bình luận
const editCmt = ref(false);
const sending = ref(false);
const listIdFileEditComments_Del = ref([]);
const Comment_ID_Edit = ref();
const addComment = () => {
    if (sending.value == true) {
        return;
    }
    if (
        (comment.value == "" ||
        comment.value == null ||
        comment.value == "<p><br></p>" ||
        comment.value == "<body><p><br></p></body>") &&
        FileAttach.value.length == 0
    ) {
        return;
    } else {
        let formData = new FormData();
        let requestComment = {
            content: "<body>" + comment.value + "</body>",
            parent_id: ReplyID.value || null,
            request_id: detail_request.value.request_id,
            request_comment_id: editCmt.value == true ? Comment_ID_Edit.value : null,
        };
        if (FileAttach.value != null) {
            for (var i = 0; i < FileAttach.value.length; i++) {
                let file = FileAttach.value[i];
                formData.append("url_file", file);
            }
        }
        formData.append("comment", JSON.stringify(requestComment));
        if (editCmt.value == true) {
            formData.append("Del_file_ID", JSON.stringify(listIdFileEditComments_Del.value));
        }
        sending.value = true;
        axios({
            method: editCmt.value ? "put" : "post",
            url:
                baseUrlCheck +
                `/api/request_comment/${ editCmt.value ? "updateComments" : "Add_Comment" }`,
            data: formData,
            headers: {
                Authorization: `Bearer ${store.getters.token}`,
            },
        })
        .then((response) => {
            swal.close();
            if (response.data.err != "1") {
                toast.success(
                    editCmt.value
                    ? "Cập nhật bình luận đề xuất thành công!"
                    : "Thêm mới bình luận đề xuất thành công!",
                );
                comment.value = "";
                comment_zone_main.value.setHTML("<p>" + comment.value + "</p>");
                filecoments = [];
                FileAttach.value = [];
                editCmt.value = false;
                sending.value = false;
                if (IsReply.value) {
                    HuyReply();
                }
                listComments();
                //GotoView("comment_final");
            } else {
                swal.fire({
                    title: "Error!",
                    text: response.data.ms,
                    icon: "error",
                    confirmButtonText: "OK",
                });
                sending.value = false;
            }
        })
        .catch(() => {
            swal.close();
            swal.fire({
            title: "Error!",
            text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
            icon: "error",
            confirmButtonText: "OK",
            });
        });
    }
};

const commentActive = ref();
const EditComment = (co) => {
	Comments.value.filter(x => x.IsEdit).forEach((r) => {
		r.IsEdit = false;
	});
	co.IsEdit = true;
    if (co.FileAttach_Edit == null) {
        co.FileAttach_Edit = [];
    }
	editCmt.value = true;
	FileAttach.value = [];
	co.Noidung_Edit = co.content;
	//commentActive.value = JSON.parse(JSON.stringify(co));
	comment.value = co.content;
	funcCmtRequest.value.toggle();
	if (co.parent_id != null) {
		Comments.value.filter(x => x.IsReply).forEach((r, idx) => {
			r.IsReply = false;
		});
		IsReply.value = true;
		ReplyID.value = co.parent_id;
		var replyMes = Comments.value.filter(x => x.request_comment_id == co.parent_id).length > 0 
                        ? Comments.value.filter(x => x.request_comment_id == co.parent_id)[0]
                        : null;
		if (replyMes != null) {
			replyMes.IsReply = true;
		}
		tinnhanreply.value = replyMes;
	}
	//goBottomChat();
};
const cancelEditComment = (u) => {
    u.IsEdit = !(u.IsEdit || false);
	editCmt.value = false;
    comment.value = "";
    FileAttach.value = [];
    comment_zone_main.value.setHTML(comment.value);
};

const funcCmtRequest = ref();
const cmtFuncClick = ref();
const toogleFuncMes = (event, mes, idx) => {
	funcCmtRequest.value.toggle(event);
	cmtFuncClick.value = mes;
	cmtFuncClick.value.indexList = idx;
};
const chooseFile = (id) => {
	document.getElementById(id).value = "";
  	document.getElementById(id).click();
};
const PutFileUpload = (event, type) => {
	var ms = false;
	var listFiles = [];
	if (!type) {
		listFiles = event.target.files;
	}
	else {
		listFiles = event;
	}
	listFiles.forEach((fi, idx) => {
		let formData = new FormData();
		formData.append("fileupload", fi);
		axios
			({
			method: 'post',
			url: basedomainURL + `/api/chat/ScanFileUpload`,
			data: formData,
			headers: {
				Authorization: `Bearer ${store.getters.token}`,
			},
		})
		.then((response) => {
			if (response.data.err != "1") {
				//toast.success("File => Ok");
				if (fi.size > 100 * 1024 * 1024) {
					ms = true;
				}
				else {
					fi.file_name = fi.name;
					fi.file_size = fi.size;
					fi.file_type = fi.name.substring(fi.name.lastIndexOf(".") + 1);
					FileAttach.value.push(fi);
				}
			} else {
				swal.fire({
					title: "Cảnh báo",
					text: "File bị xóa do tồn tại mối đe dọa với hệ thống!",
					icon: "warning",
					confirmButtonText: "OK",
				});
			}
			if (ms) {
				swal.fire({
					icon: 'warning',
					type: 'warning',
					title: 'Thông báo',
					text: 'Bạn chỉ được upload file có dung lượng tối đa 100MB!'
				});
			}
			
		})
		.catch((error) => {
			swal.fire({
				title: "Thông báo",
				text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
				icon: "error",
				confirmButtonText: "OK",
			});
		});
	});
};

const FileAttach = ref([]);
const tinnhanreply = ref({});
const ReplyID = ref();
const Reply = (co) => {
    Comments.value.filter(x => x.IsReply).forEach((r, idx) => {
		r.IsReply = false;
	});
    IsReply.value = true;
	ReplyID.value = co.request_comment_id;
	co.IsReply = true;
	tinnhanreply.value = co;
};
const HuyReply = () => {
	var co = Comments.value.find(x => x.request_comment_id === ReplyID.value);
	IsReply.value = false;
	ReplyID.value = null;
	tinnhanreply.value = null;
	co.IsReply = false;
	FileAttach.value = [];
};
const removeFilesComment = (files, i) => {
	files.splice(i, 1);
};
const Del_Comment = (msg, i) => {

};

// Function request
const printRequest = () => {

};
const editRequest = () => {

};
const delRequest = (dataR, type) => {

};
const openRecallRequest = (dataRequest, f) => {
    
};
// ---
// Function xử lý request
// Công việc
const RQJobs = ref([]);
const viewXLDX = (dex) => {
    dex.IsViewXL = !(dex.IsViewXL || false);
    for (var i = 0; i < RQJobs.value.length; i++) {
        RQJobs.value[i].isCurrent = null;
    }
    if (dex.IsViewXL === true) {
        RQJobs.value = [];
        listJob(dex);
    }
};
const setCurrent = (state) => {
    if (state.isCurrent == true) {
        for (var i = 0; i < RQJobs.value.length; i++) {
            RQJobs.value[i].isCurrent = null;
        }
    } else {
        for (var i = 0; i < RQJobs.value.length; i++) {
            RQJobs.value[i].isCurrent = false;
        }
        state.isCurrent = true;
    }
    detail_request.value.IsViewXL = true;
};

const listJob = (dex) => {
    axios
        .post(
            basedomainURL + "api/request/getData",
            { 
                str: encr(JSON.stringify({
                        proc: "request_job_get",
                        par: [
                            { par: "request_id", va: detail_request.value.request_id },
                            { par: "user_id", va: store.getters.user.user_id },
                        ],
                    }), SecretKey, cryoptojs
                ).toString()
            },
            config
        )
        .then((response) => {
            if (response.data.err != '1') {
                var data = JSON.parse(res.data.data);
                data[0].forEach((t) => {
                    var ot = colorTT.find(x => x.id == t.status);
                    if (ot) {
                        t.TrangthaiTen = ot.text;
                        t.TrangthaiColor = ot.color;
                    }
                    t.ListTasks = data[1].filter(x => x.request_job_id == t.request_job_id);
                    t.ListTasks.forEach((r) => {
                        if (r.Thanhviens) {
                            r.Thanhviens = JSON.parse(r.Thanhviens);
                        }
                        if (r.Tags) {
                            r.Tags = JSON.parse(r.Tags);
                        }
                        r.NgayHan = Math.abs(r.NgayHan);
                        if (r.Thanhviens) {
                            r.IsQL = r.Thanhviens.filter(x => x.user_id === store.getters.user.user_id && x.is_type === "2").length > 0;
                            r.IsTH = r.Thanhviens.filter(x => x.user_id === store.getters.user.user_id && x.is_type === "1").length > 0;
                            r.IsTD = r.Thanhviens.filter(x => x.user_id === store.getters.user.user_id && x.is_type === "0").length > 0;
                            r.IsType = r.IsQL ? 2 : (r.IsTH ? 1 : 0);
                        }
                    });
                });
                RQJobs.value = data[0];
            }
        });
};
const MoveJob = (job, f) => {
    let idx = RQJobs.value.findIndex(x => x.request_job_id == job.request_job_id);
    var id = RQJobs.value[idx].request_job_id;
    var dx = !f ? idx + 1 : idx - 1;
    var STT = RQJobs.value[idx].is_order;
    var tid = RQJobs.value[dx].request_job_id;
    RQJobs.value[idx].is_order = RQJobs.value[dx].is_order;
    RQJobs.value[dx].is_order = STT;
    axios({
        method: "post",
        url: basedomainURL + "api/Request/Move_Job",
        data: { id: id, tid: tid },
        headers: {
            Authorization: `Bearer ${store.getters.token}`,
        },
    }).then((response) => {
        if (response.data.err != '1') {
            toast.success("Đổi thứ tự nhiệm vụ thành công!.");
        } else {
            swal.fire({
                icon: 'error',
                type: 'error',
                title: '',
                text: 'Đổi thứ tự nhiệm vụ không thành công, vui lòng thử lại!'
            });
        }
    });
};
const MoveJobTask = (jobTask, task, f) => {
    let idx = jobTask.findIndex(x => x.request_job_task_id == task.request_job_task_id);
    var id = jobTask[idx].request_job_task_id;
    var dx = !f ? idx + 1 : idx - 1;
    var STT = jobTask[idx].is_order;
    var tid = jobTask[dx].request_job_task_id;
    jobTask[idx].is_order = jobTask[dx].is_order;
    jobTask[dx].is_order = STT;
    axios({
        method: "post",
        url: basedomainURL + "api/Request/Move_Job_Task",
        data: { id: id, tid: tid },
        headers: {
            Authorization: `Bearer ${store.getters.token}`,
        },
    }).then((response) => {
        if (response.data.err != '1') {
            toast.success("Đổi thứ tự công việc thành công!.");
        } else {
            swal.fire({
                icon: 'error',
                type: 'error',
                title: '',
                text: 'Đổi thứ tự công việc không thành công, vui lòng thử lại!'
            });
        }
    });
};
const colorTT = ref([
    { id: 0, text: "Chưa bắt đầu", color: "#aaa" },
    { id: 1, text: "Đang làm", color: "#2196f3" },
    { id: 2, text: "Hoàn thành", color: "#6dd230" },
    { id: 3, text: "Chuyển tiếp", color: "#33c9dc" },
    { id: 4, text: "Tạm dừng", color: "#fe4d97" },
    { id: 5, text: "Không hoàn thành", color: "red" }
]);
// type: 1=Chuyển xử lý/Xử lý tiếp, 2=Đánh giá đề xuất/Gửi người lập đánh giá, 4=Dừng xử lý
const openXLDX = (dataR, type, text) => {

};
const openFlow = (dataR) => {

};

// type: 1=Chấp thuận, -1=Từ chối, 2=Chuyển tiếp, 3=Đồng ý & chuyển tiếp, null=Gửi
const OpenSendRequest = (dataR, text, type) => {

};
// Huỷ request
const StopRequest = (dataR) => {

};
// Bỏ hủy request
const BackRequest = (dataR) => {

};
// Gia hạn request
const openModalDatelineRequest = (dataR) => {

};

const toggleMores = (event, item) => {
    menuButMores.value.toggle(event);
};
const menuButMores = ref();
const itemButMores = ref([
    {
        label: "Thiết lập quy trình xử lý",
        icon: "pi pi-cog",
        class: "",
        command: (event) => {
            openFlow(detail_request);
        },
    },
    {
        label: "Chuyển bộ phận đề xuất xử lý",
        icon: "pi pi-send",
        class: "status-process-0",
        command: (event) => {
            openXLDX(detail_request, 1, 'Chuyển xử lý');
        },
    },
    {
        label: detail_request.created_by == store.getters.user.user_id ? 'Đánh giá đề xuất' : 'Gửi người lập đánh giá',
        icon: "pi pi-user",
        class: "status-process-1",
        command: (event) => {
            openXLDX(detail_request, 2, 'Gửi người lập đánh giá');
        },
    },
    {
        label: "Dừng xử lý",
        icon: "pi pi-stop-circle",
        class: "status-process-4",
        command: (event) => {
            openXLDX(detail_request, 4, 'Dừng xử lý');
        },
    },
    {
        label: "Xử lý tiếp",
        icon: "pi pi-play",
        class: "status-process-4",
        command: (event) => {
            openXLDX(detail_request, 1, 'Xử lý tiếp');
        },
    },
]);
const getFuncRequest = () => {
    if (detail_request.value.status_processing == 0) {
        return itemButMores.filter(x => x.class == "" || x.class.includes("status-process-0"));
    }
    else if (detail_request.value.status_processing == 1) {
        return itemButMores.filter(x => x.class == "" || x.class.includes("status-process-1"));
    }
    else if (detail_request.value.status_processing == 4) {
        return itemButMores.filter(x => x.class == "" || x.class.includes("status-process-4"));
    }
    if (detail_request.value.status_processing != 4) {
        return itemButMores.filter(x => !x.class.includes("status-process-4"));
    }
    return itemButMores;
};

// Đề xuất liên quan
const RelateRequests = ref([]);
// Danh sách đề xuất liên quan
const openRelate = (dataRelate, module, type) => {
    
};
// Chi tiết đề xuất liên quan đã chọn
const openURLRQ = (r) => {

};
// Xóa đề xuất liên quan
const Del_Relate = (text, id_relate, dataRelate) => {

};

// ---
// Danh sách ký duyệt
const showChartSign = (r) => {

};
// File đính kèm request
const openModalAddFileCV = (dataR) => {

};
// Xóa file đính kèm
const Del_AttachFile = (listFiles, fileDel) => {

};

// Download file
const downloadFile = (file) => {
    let name = "";
    let pathFileDownload = "";
    if (file.files != null && file.files.length > 0) {
        name = file.files[0].file_name || ("file_download" + file.files[0].file_type);
        pathFileDownload = file.files[0].file_path;
    }
    else {
        pathFileDownload = file.file_path;
		name = file.file_name || ("file_download." + file.file_type);
    }    
    const a = document.createElement("a");
    a.href = basedomainURL + '/Viewer/DownloadFile?url='+ encodeURIComponent(pathFileDownload) + '&title=' + encodeURIComponent(name);
    a.download = name;
    // a.target = "_blank";
    a.click();
    a.remove();
}
// show file
const displayModalIframeReq = ref(false);
const fileShow = ref({
	file_name: "",
	file_path: "",
});
const typeShow = ref(2);
const showfile = (file, cmt) => {
    if (cmt == null || (cmt != null && cmt.type_comment != 3 && cmt.type_comment != 4)) {
		fileShow.value.file_name = file.file_name;
		typeShow.value = 2;	
	}
	else if (cmt != null && cmt.type_comment == 3) {
		typeShow.value = 3;
	}
	else if (cmt != null && cmt.type_comment == 4) {
		typeShow.value = 4;
	}
	fileShow.value.file_path = file.file_path;
	fileShow.value.file_type = file.file_type;
	displayModalIframeReq.value = true;
};

// Right sidebar
const tabLogActive = ref(0);
const changeTabContent = (event) => {
	tabLogActive.value = event.index;
}

const dataQT = ref([]);
const listQT_Request = () => {

};
const dataLog = ref([]);
const listLog = () => {

};
// ---

const hideall = () => {
    emitter.emit("SideBarRequest", false);
};
const PositionSideBar = ref("right");
const MaxMin = (m) => {
    PositionSideBar.value = m;
    emitter.emit("psbRequest", m);
};
const closeSildeBar = () => {
    emitter.emit("SideBarRequest", false);
};

const is_viewSecurityRequest = ref(true);
onMounted(() => {
    if (props.id != null) {
        loadData(true);
        listComments();
        loadEmote();
    }
    else {
        hideall();
    }
    return {};
});
</script>
<template>
    <div class="overflow-hidden h-full w-full col-12 p-0 m-0 flex"
        v-if="is_viewSecurityRequest == true"
    >
        <div class="col-8 md:col-8 p-0 pl-2 pr-2 m-0" 
            style="border-right: 5px solid #ededed;" 
            v-if="detail_request != null"
        >
            <div class="row col-12 flex justify-content-center px-0 mx-0 format-center">
                <div class="col-1 p-0 m-0 flex">
                    <Button
                        icon="pi pi-times"
                        class="p-button-rounded p-button-text"
                        v-tooltip="{ value: 'Đóng' }"
                        @click="closeSildeBar()"
                    />
                    <Button
                        icon="pi pi-window-maximize"
                        class="p-button-rounded p-button-text"
                        v-tooltip="{ value: 'Phóng to' }"
                        @click="MaxMin('full')"
                        v-if="PositionSideBar == 'right'"
                    />
                    <Button
                        icon="pi pi-window-minimize"
                        class="p-button-rounded p-button-text"
                        v-tooltip="{ value: 'Thu nhỏ' }"
                        @click="MaxMin('right')"
                        v-if="PositionSideBar == 'full'"
                    />
                </div>
                <div class="col-11 p-0 pl-3 m-0 flex" style="justify-content: space-between;">
                    <div class="flex" style="align-items: center;">
                        <i class="pi pi-check-square pr-2"></i>
                        <span class="font-bold" style="font-size: 1.25rem;">{{ detail_request.request_name }}</span>
                        <span class="card-nhom text-left"
                            style="padding:0.25rem 0.5rem;margin-left:0.5rem !important;background-color:#0078d4;color:#ffffff;" 
                        >
                            {{ detail_request.request_code || '' }}
                        </span>
                    </div>
                    <div class="flex" style="align-items: center;">
                        <Button
                            icon="pi pi-print"
                            class="p-button-rounded p-button-text"
                            v-tooltip="{ value: 'In' }"
                            @click="printRequest()"
                            v-if="false"
                        />
                        <Button
                            icon="pi pi-pencil"
                            class="p-button-rounded p-button-text"
                            v-tooltip="{ value: 'Chỉnh sửa' }"
                            @click="editRequest()"
                            v-if="false"
                        />
                        <Button
                            icon="pi pi-trash"
                            class="p-button-rounded p-button-text"
                            style="color:red;"
                            v-tooltip="{ value: 'Xóa' }"
                            @click="delRequest()"
                            v-if="false"
                        />
                        <span class="card-nhom text-left"
                            style="padding:0.25rem 0.5rem;margin-left:0.5rem !important;background-color:#ff8b4e;color:#ffffff;"
                            v-if="detail_request.is_change_process"
                        >
                            Quy trình động
                        </span>
                    </div>
                </div>
            </div>
            <div class="row col-12 p-0 pl-2" style="position: relative;flex-direction: column;height: calc(100vh - 4rem);">
                <div class="col-12 flex">
                    <span class="pr-2" style="font-style: italic;">Loại đề xuất:</span>
                    <span class="font-bold" style="font-style: italic;">{{ detail_request.request_form_name || 'Đề xuất trực tiếp' }}</span>
                </div>
                <div class="col-12 flex">
                    <span class="pr-2">Tạo bởi:</span>
                    <span class="font-bold" style="color:#2196f3;">{{ (detail_request.full_name || '') }}</span>
                    <span class="pl-2">
                        {{ " - " + (detail_request.created_date ? moment(new Date(detail_request.created_date)).format('HH:mm DD/MM/yyyy') : '') }}
                    </span>
                </div>
                <div class="col-12 flex">
                    <div class="requestbutton" 
                        style="max-width:30%" 
                        v-if="detail_request.is_create && !detail_request.is_func && detail_request.status_processing == 1 && detail_request.Tiendo <= 0"
                    >
                        <Button class="p-button-warning" 
                            style="background-color:orange"
                            @click="openRecallRequest(detail_request, true)"
                            label="Thu hồi">
                        </Button>
                    </div>
                    <div class="requestbutton" 
                        style="max-width:100%;height:36px" 
                        v-if="detail_request.status_processing == 2 && detail_request.is_evaluate == true">
                        <div style="flex:1">
                            <div style="display:flex">
                                <div style="margin-right:10px" class="btn-group requestbutton" 
                                    v-if="detail_request.status_processing == 2 && detail_request.status_processing > 0">
                                    <Button label="Đang được xử lý" 
                                        @click="viewXLDX(detail_request)" 
                                        style="margin-right:0" 
                                        v-if="detail_request.status_processing == 1">
                                    </Button>
                                    <Button label="Đang đợi xử lý" 
                                        @click="viewXLDX(detail_request)" 
                                        style="margin-right:0" 
                                        v-if="detail_request.status_processing == 0" 
                                        class="p-button-secondary">
                                    </Button>
                                    <Button label="Đã hoàn thành xử lý" 
                                        @click="viewXLDX(detail_request)" 
                                        style="margin-right:0;border-top-right-radius:5px;border-bottom-right-radius:5px" 
                                        v-if="detail_request.status_processing == 3" 
                                        class="p-button-success">
                                    </Button>
                                    <Button label="Đánh giá đề xuất" 
                                        v-if="detail_request.is_create && detail_request.status_processing == 2" 
                                        @click="openXLDX(detail_request,2,'Đánh giá đề xuất')" 
                                        style="background-color:#00bcd4;border-top-right-radius:5px;border-bottom-right-radius:5px">
                                    </Button>
                                    <Button label="Chờ người lập đánh giá" 
                                        v-if="!detail_request.is_create && detail_request.status_processing == 2" 
                                        @click="viewXLDX(detail_request)" 
                                        style="margin-right:0;background-color:orange">
                                    </Button>
                                    <Button label="Dừng xử lý" 
                                        v-if="(detail_request.IsXL||detail_request.IsSignXL) && detail_request.status_processing == 4" 
                                        @click="viewXLDX(detail_request)" 
                                        style="margin-right:0;" 
                                        class="p-button-danger">
                                    </Button>
                                    <Button
                                        v-if="!detail_request.is_create && (detail_request.IsXL || detail_request.IsSignXL) && 
                                        ((detail_request.status_processing != 3 && detail_request.status_processing != 2) || (detail_request.status_processing == 2 && detail_request.created_by === store.getters.user.user_id))"
                                        icon="pi pi-ellipsis-h"
                                        :class="'p-button-' + (detail_request.status_processing == 0 ? 'secondary' 
                                                                : detail_request.status_processing == 1 ? 'primary' 
                                                                : detail_request.status_processing == 2 ? 'success'
                                                                : 'danger')"
                                        @click="toggleMores($event, detail_request)"
                                        aria-haspopup="true"
                                        aria-controls="overlay_More"
                                        v-tooltip.top="'Tác vụ'"
                                    />
                                    <Menu 
                                        class="menu-request"
                                        id="overlay_More"
                                        ref="menuButMores"
                                        :model="getFuncRequest()"
                                        :popup="true"
                                    />                                
                                </div>
                            </div>
                        </div>
                        <div class="wizard small ng-cloak" 
                            v-if="detail_request.status_processing == 2 && RQJobs.length > 1">
                            <template v-for="(state, idxJob) in RQJobs">
                                <a v-tooltip.top="{ value: state.job_name }" 
                                    @click="setCurrent(state)" 
                                    :class="(state.isCurrent ? 'current': '') + ' ' + ('job' + state.status)">
                                    <span>{{ idxJob + 1 }}</span>
                                </a>
                            </template>
                        </div>
                        <a @click="viewXLDX(detail_request)" 
                            style="color:#2196f3 !important;font-size:12px;margin-left:20px" 
                            v-if="detail_request.Stask > 0"
                        >
                            <font-awesome-icon icon="fa-solid fa-list-check" /> ({{ (detail_request.StaskFinish || 0) + "/" + (detail_request.Stask || 0) }})
                            <div v-tooltip.top="{ value: 'Tiến độ công việc: ' + (detail_request.StaskTiendo || 0) + '%' }" 
                                class="radialProgressBar"
                                :class="detail_request.bgtiendo ? ('progress-' + detail_request.bgtiendo) : ''" 
                                style="width:24px;height:24px;display:inline-flex">
                                <div class="overlay" 
                                    style="font-size:6px;width:16px;height:16px;color:#000">
                                    {{ detail_request.StaskTiendo || 0 }}%
                                </div>
                            </div>
                        </a>
                    </div>
                    <div class="requestbutton" 
                        v-if="!detail_request.is_create && detail_request.is_func && detail_request.status_processing == 1"
                    >
                        <Button label="Chấp thuận" 
                            @click="OpenSendRequest(detail_request,'Chấp thuận',1)" 
                            class="p-button-success">
                        </Button>
                        <Button label="Từ chối"
                            @click="OpenSendRequest(detail_request, 'Từ chối', -1)" 
                            class="p-button-danger">
                        </Button>
                        <Button v-if="!detail_request.IsLast" 
                            label="Chuyển tiếp"
                            @click="OpenSendRequest(detail_request,'Chuyển tiếp',2)">
                        </Button>
                        <Button v-if="detail_request.IsForward" 
                            label="Đồng ý & chuyển tiếp"
                            @click="OpenSendRequest(detail_request,'Đồng ý và chuyển tiếp',3)" 
                            class="p-button-success">
                        </Button>
                    </div>
                    <div class="requestbutton" 
                        v-if="detail_request.is_create && detail_request.is_func"
                    >
                        <Button v-if="!detail_request.IsHoanthanh" 
                            label="Gửi" 
                            @click="OpenSendRequest(detail_request,'Gửi')">
                        </Button>
                        <Button v-if="detail_request.status_processing != -1 && detail_request.status_processing != 0" 
                            label="Hủy"
                            @click="StopRequest(detail_request)" 
                            class="p-button-warning" 
                            style="background-color:orange">
                        </Button>
                        <Button v-if="detail_request.status_processing == -1" 
                            label="Bỏ hủy"
                            @click="BackRequest(detail_request)" 
                            class="p-button-warning" 
                            style="background-color:#6fbf73">
                        </Button>
                        <Button label="Xóa"
                            @click="delRequest(detail_request, 1)" 
                            class="p-button-danger">
                        </Button>
                    </div>
                </div>
                <div class="col-12 flex" v-if="detail_request.IsViewXL">
                    <div class="job-title">
                        <label>
                            <font-awesome-icon icon="fa-solid fa-list-check" />
                            {{RQJobs.length}} nhiệm vụ đang thực hiện
                        </label> 
                        <a v-if="(detail_request.IsXL || detail_request.IsSignXL) && detail_request.status_processing != 3" 
                            @click="openFlow(detail_request)">
                            <i class="pi pi-plus-circle pr-2"></i> 
                            Thêm nhiệm vụ
                        </a>
                    </div>
                </div>
                <div class="col-12 flex" style="flex-direction: column;">
                    <div class="task-content scrollbox_delayed w-full" 
                        style="padding:0.75rem 0;overflow-y: auto;overflow-x:hidden;"
                        :style="detail_request.IsViewXL ? 'height: calc(100vh - 225px)' : 
                                FileAttach.length > 0 ? 'height: calc(100vh - 440px)' : 
                                !IsReply ? 'height: calc(100vh - 250px)' : 
                                detail_request.IsComment ? 'height: calc(100vh - 330px)' : 'height: calc(100vh - 145px)'
                        "
                        id="request_message_panel"
                    >
                        <div v-show="!detail_request.IsViewXL" 
                            class="scrollbox-content"
                            style="margin-right:0;"
                        >
                            <form id="frRequest">
                                <div class="row">
                                    <div class="col-3 p-0" v-if="true || detail_request.modified_date">
                                        <div class="t-r">
                                            <div class="flex">
                                                <span class="cv-spicon flex" style="align-items:center;">
                                                    <i class="pi pi-clock"></i>
                                                </span>
                                                <span class="cv-request">Cập nhật</span>
                                            </div>
                                        </div>
                                        <p>
                                            <span class="datetime-request">
                                                {{ detail_request.modified_date ? moment(new Date(detail_request.modified_date)).format("HH:mm DD/MM/yyyy") : (detail_request.created_date ? moment(new Date(detail_request.created_date)).format("HH:mm DD/MM/yyyy") : '') }}
                                            </span>
                                        </p>
                                    </div>
                                    <div class="col-3 p-0">
                                        <div class="t-r">
                                            <div class="flex">
                                                <span class="cv-spicon flex" style="align-items:center;">
                                                    <i class="pi pi-calendar"></i>
                                                </span>
                                                <span class="cv-request">Ngày lập</span>
                                            </div>
                                        </div>
                                        <p>
                                            <span class="datetime-request">
                                                {{ detail_request.created_date ? moment(new Date(detail_request.created_date)).format("HH:mm DD/MM/yyyy") : '' }}
                                            </span>
                                        </p>
                                    </div>
                                    <div class="col-3 p-0" v-if="true || detail_request.deadline_date != null">
                                        <div class="t-r">
                                            <div class="flex">
                                                <span class="cv-spicon flex" style="align-items:center;">
                                                    <i class="pi pi-calendar-times"></i>
                                                </span>
                                                <span class="cv-request">Hạn xử lý</span>
                                            </div>
                                        </div>
                                        <p>
                                            <span class="datetime-request" style="color:#2196f3">
                                                {{ detail_request.deadline_date ? moment(new Date(detail_request.deadline_date)).format("HH:mm DD/MM/yyyy") : '' }}
                                            </span>
                                        </p>
                                    </div>
                                    <div class="col-3 p-0" v-if="detail_request.status_processing == 2">
                                        <div class="t-r">
                                            <div class="flex">
                                                <span class="cv-spicon flex" style="align-items:center;">
                                                    <i class="pi pi-clock"></i>
                                                </span>
                                                <span class="cv-request">Hoàn thành</span>
                                            </div>
                                        </div>
                                        <p>
                                            <span class="datetime-request">
                                                {{ detail_request.completed_date ? moment(new Date(detail_request.completed_date)).format("HH:mm DD/MM/yyyy") : '' }}
                                            </span>
                                        </p>
                                    </div>
                                </div>
                                <div class="row" 
                                    style="flex-direction: column;margin-top:0.5rem" 
                                    v-if="detail_request.content"
                                >
                                    <div class="t-r">
                                        <div class="flex">
                                            <span class="cv-spicon flex" style="align-items:center;">
                                                <i class="pi pi-align-left"></i>
                                            </span>
                                            <span class="cv-request">Nội dung</span>
                                        </div>
                                    </div>
                                    <p style="margin-left: 22px" v-html="detail_request.content"></p>
                                </div>
                                <div class="row" v-if="detail_request.Tiendo > 0">
                                    <div class="col-12 p-0">
                                        <div class="t-r">
                                            <div class="flex">
                                                <span class="cv-spicon flex" style="align-items:center;">
                                                    <font-awesome-icon icon="fa-solid fa-list-check" />
                                                </span>
                                                <span class="cv-request">Tiến độ</span>
                                            </div>
                                        </div>
                                        <div class="flex my-3">                                            
                                            <span class="flex font-bold mr-3" style="font-size:1.2rem;margin-left:22px;">
                                                {{ detail_request.Tiendo }}% 
                                            </span>
                                            <ProgressBar class="progress-bar-custom flex"
                                                :class="renderColorProgress(detail_request.Tiendo)"
                                                v-tooltip.top="{ value: (detail_request.Tiendo + '%') }" 
                                                :value="(detail_request.Tiendo || 0)"
                                                style="flex:1;">
                                            </ProgressBar>
                                        </div>
                                    </div>
                                </div>
                                
                                <div class="row mt-2" 
                                    style="flex-direction:column;"
                                    v-if="true || (detail_request.status_processing == 2 || detail_request.status_processing == 3)"
                                >
                                    <div class="t-r">
                                        <div class="flex">
                                            <span class="cv-spicon flex" style="align-items:center;">
                                                <i class="pi pi-star"></i>
                                            </span>
                                            <span class="cv-request">Đánh giá đề xuất</span>
                                        </div>
                                    </div>
                                    <div class="flex p-3" v-if="true || detail_request.avatar_completed_all">
                                        <div class="r-ava">
                                            <Avatar
                                                v-bind:label="
                                                    detail_request.avatar_completed_all
                                                    ? ''
                                                    : (detail_request.last_name_completed_all ?? '').substring(0, 1).toUpperCase()
                                                "
                                                v-bind:image="
                                                    detail_request.avatar_completed_all
                                                    ? basedomainURL + detail_request.avatar_completed_all
                                                    : basedomainURL + '/Portals/Image/nouser1.png'
                                                "
                                                v-tooltip.top="{ value: (detail_request.full_name_completed_all + '<br/>' + detail_request.position_name_completed_all + '<br/>' + detail_request.department_name_completed_all), escape: true }"
                                                style="color: #ffffff; font-size: 1rem !important;"
                                                :style="{ background: bgColor[0], }"
                                                size="large"
                                                shape="circle"
                                                class="border-radius"
                                            />
                                        </div>
                                        <div class="flex ml-3" style="flex-direction: column;">
                                            <div class="r-cname">
                                                <span class="font-bold mr-3">{{ detail_request.full_name_completed_all || '' }}</span>
                                                <span class="request-cdate">
                                                    {{ detail_request.completed_all_date ? moment(new Date(detail_request.completed_all_date)).format("HH:mm DD/MM/yyyy") : '' }}
                                                </span>
                                            </div>
                                            <div class="mt-2" style="word-break: break-word;">
                                                <div style="margin-bottom:0" v-html="detail_request.completed_all_content || ''"></div>
                                            </div>
                                        </div>
                                    </div>
                                    <div class="flex p-3" v-if="true || (detail_request.evaluated_score > 0)">
                                        <div class="r-ava">
                                            <Avatar
                                                v-bind:label="
                                                    detail_request.avatar_evaluated
                                                    ? ''
                                                    : (detail_request.last_name_evaluated ?? '').substring(0, 1).toUpperCase()
                                                "
                                                v-bind:image="
                                                    detail_request.avatar_evaluated
                                                    ? basedomainURL + detail_request.avatar_evaluated
                                                    : basedomainURL + '/Portals/Image/nouser1.png'
                                                "
                                                v-tooltip.top="{ value: (detail_request.full_name_evaluated + '<br/>' + detail_request.position_name_evaluated + '<br/>' + detail_request.department_name_evaluated), escape: true }"
                                                style="color: #ffffff; font-size: 1rem !important;"
                                                :style="{ background: bgColor[0], }"
                                                size="large"
                                                shape="circle"
                                                class="border-radius"
                                            />
                                        </div>
                                        <div class="flex ml-3" style="flex-direction: column;">
                                            <div class="r-cname">
                                                <span class="font-bold mr-3">{{ detail_request.full_name_evaluated || '' }}</span>
                                                <span class="request-cdate">
                                                    {{ detail_request.evaluated_date ? moment(new Date(detail_request.evaluated_date)).format("HH:mm DD/MM/yyyy") : '' }}
                                                </span>
                                            </div>
                                            <div class="mt-2" v-if="true || detail_request.status_processing == 3">
                                                <Rating class="star-rating-custom"
                                                    v-model="detail_request.evaluated_score"
                                                    v-tooltip.top="{ value: 'Ngày đánh giá: <br/>' + (detail_request.evaluated_date ? moment(new Date(detail_request.evaluated_date)).format('HH:mm DD/MM/yyyy') : ''), escape: true }"
                                                    :stars="5"
                                                    :cancel="false" 
                                                    :readonly="true"
                                                />
                                            </div>
                                            <div class="mt-2" style="word-break: break-word;">
                                                <div style="margin-bottom:0" v-html="detail_request.evaluated_content || ''"></div>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                                
                                <div class="row mt-2" v-if="FormDS && formDS_filter().length>0">
                                    <div class="col-12 flex p-0" style="flex-wrap: wrap;">
                                        <div class="formd pl-0"
                                            :class="(!d.is_class ? 'pr-0': (d.is_class == 'col-12' ? (d.is_class + ' pr-0') : d.is_class)) + ' ' + (d.is_end_line ? 'pr-0' : '')"
                                            v-for="(d, idxForm) in formDS_filter(null)"
                                            :key="idxForm"
                                        >
                                            <div v-if="d.is_type != 3">
                                                <div class="form-group formlabel" v-if="d.is_label">
                                                    {{ d.ten_truong }}
                                                </div>
                                                <div class="form-group" v-else>
                                                    <div class="form-group flex mb-0" 
                                                        v-if="d.kieu_truong != 'checkbox' && d.kieu_truong != 'radio' && d.kieu_truong != 'switch' && d.is_type != 2"
                                                    >
                                                        <label>{{ d.ten_truong }}</label>
                                                        <span v-if="d.is_required" class="redsao pl-1">(*)</span> 
                                                    </div>
                                                    <div class="form-group flex mb-0" v-else>
                                                        <label style="height: 1rem;"></label>
                                                    </div>
                                                    <div v-if="d.kieu_truong">
                                                        <div v-if="d.kieu_truong == 'email'">
                                                            <InputText :max="d.is_length" 
                                                                type="email" 
                                                                spellcheck="false" 
                                                                v-model="d.value_field"
                                                                class="form-control col-12 ip36 p-2"
                                                                :class="{ 'p-invalid': d.is_required && !d.value_field && submitted, }"
                                                            />
                                                        </div>
                                                        <div v-if="d.kieu_truong == 'varchar' || d.kieu_truong == 'nvarchar'">
                                                            <InputText :max="d.is_length" 
                                                                type="text" 
                                                                spellcheck="false" 
                                                                v-model="d.value_field"
                                                                class="form-control col-12 ip36 p-2"
                                                                :class="{ 'p-invalid': d.is_required && !d.value_field && submitted, }"
                                                            />
                                                        </div>
                                                        <div v-if="d.kieu_truong == 'int' || d.kieu_truong == 'float'">
                                                            <InputNumber
                                                                spellcheck="false" 
                                                                v-model="d.value_field" 
                                                                class="form-control col-12 ip36 p-2"
                                                                :class="{ 'p-invalid': d.is_required && !d.value_field && submitted, }"
                                                            />
                                                        </div>
                                                        <div v-if="d.kieu_truong == 'textarea'">
                                                            <Textarea :max="d.is_length" 
                                                                spellcheck="false" 
                                                                v-model="d.value_field" 
                                                                class="form-control col-12 p-2"
                                                                :class="{ 'p-invalid': d.is_required && !d.value_field && submitted, }"
                                                                rows="2"
                                                                autoResize
                                                            />
                                                        </div>
                                                        <div v-if="d.kieu_truong == 'switch'">
                                                            <div class="flex ip36 mb-0" 
                                                                style="align-items: center; flex-direction: row;">
                                                                <InputSwitch v-model="d.value_field" />
                                                                <label class="ml-2">{{ d.ten_truong }}</label>
                                                            </div>
                                                        </div>
                                                        <div v-if="d.kieu_truong == 'checkbox'">
                                                            <div class="flex ip36 mb-0" 
                                                                style="align-items: center; flex-direction: row;">
                                                                <Checkbox v-model="d.value_field" :binary="true" />
                                                                <!-- <label class="ml-2">{{ td.ten_truong }}</label> -->
                                                            </div>
                                                        </div>
                                                        <div v-if="d.kieu_truong == 'radio'">
                                                            <div class="flex ip36 mb-0" 
                                                                style="align-items: center; flex-direction: row;">
                                                                <RadioButton :value="d.request_formd_id" 
                                                                    v-model="request_data.Radio"/>
                                                                <label class="ml-2">{{ d.ten_truong }}</label>
                                                            </div>
                                                        </div>
                                                        <div v-if="d.kieu_truong == 'select' && d.is_type == 9">
                                                            <Dropdown
                                                                :options="list_type_dayoff"
                                                                v-model="d.value_field"
                                                                optionLabel="name" 
                                                                optionValue="code" 
                                                                placeholder="-- Loại nghỉ --"
                                                            >
                                                            </Dropdown>
                                                        </div>
                                                        <div v-if="d.kieu_truong == 'date' || d.kieu_truong == 'datetime'">
                                                            <Calendar
                                                                :showIcon="true"
                                                                class="form-control col-12 ip36 p-0"
                                                                autocomplete="on"
                                                                inputId="time24"
                                                                v-model="d.value_field"
                                                                placeholder="dd/mm/yyyy"
                                                                :class="{ 'p-invalid': d.is_required && !d.value_field && submitted, }"
                                                            />
                                                        </div>
                                                        <div v-if="d.kieu_truong == 'time'">
                                                            <!-- <Input type="text" class="form-control" v-model="d.value_field" placeholder="HH:MM:SS" onkeypress="formatTime(this)" max="8" :required="d.is_required" /> -->
                                                            <Calendar
                                                                :showIcon="true"
                                                                class="form-control col-12 ip36 p-0"
                                                                autocomplete="on"
                                                                inputId="time24"
                                                                v-model="d.value_field"
                                                                placeholder="HH:mm"
                                                                timeOnly
                                                                :class="{ 'p-invalid': d.is_required && !d.value_field && submitted, }"
                                                            />
                                                        </div>
                                                    </div>
                                                </div>
                                                <div v-if="formDS_filter(d.request_formd_id).length > 0">
                                                    <div class="formd" 
                                                        :class="dc.is_class || ''"
                                                        v-for="(dc, idxChildForm) in formDS_filter(d.request_formd_id)"
                                                        :key="idxChildForm" 
                                                    ></div>
                                                </div>
                                            </div>
                                            <div v-if="d.is_type == 3">
                                                <div class="form-group" v-if="d.is_label">
                                                    <div class="form-group formlabel" 
                                                        style="margin-bottom:0.25rem;display:flex;align-items: center;"
                                                    >
                                                        <label class="mb-0">{{ d.ten_truong }}</label>
                                                        <div style="flex:1"></div>
                                                        <!-- <Button v-if="request_data.IsEdit && request_data.is_general_request" 
                                                            @click="openRelate(null,'srequest',0)"
                                                        >
                                                            <i class="pi pi-sliders-h"></i>
                                                            <span class="pl-2">Tổng hợp đề xuất</span>
                                                        </Button> -->
                                                    </div>
                                                    <table class="table table-bordered" style="border-spacing: 0;">
                                                        <thead style="background-color:#eee">
                                                            <tr>
                                                                <template v-for="(dc, idxChildForm) in formDS_filter(d.request_formd_id)"
                                                                    :key="idxChildForm">
                                                                    <th class="th-table-render"
                                                                        :width="dc.is_width != null && dc.is_width > 0 ? dc.is_width : renderWidth(dc.kieu_truong)"
                                                                    >
                                                                        {{dc.ten_truong}}
                                                                    </th>
                                                                </template>
                                                                <!-- <th class="th-table-render" width="40"></th> -->
                                                            </tr>
                                                        </thead>
                                                        <tbody>
                                                            <tr v-for="(r, indexF) in Ftables[idxForm]" :key="indexF">
                                                                <td class="td-table-render" v-for="td in r">
                                                                    <div v-if="td.kieu_truong">
                                                                        <div v-if="td.kieu_truong == 'email'">
                                                                            <InputText :max="td.is_length" 
                                                                                type="email" 
                                                                                spellcheck="false" 
                                                                                v-model="td.value_field"
                                                                                class="form-control col-12 ip36 p-2"
                                                                                disabled
                                                                            />
                                                                        </div>
                                                                        <div v-if="td.kieu_truong == 'varchar' || td.kieu_truong == 'nvarchar'">
                                                                            <InputText :max="td.is_length" 
                                                                                type="text" 
                                                                                spellcheck="false" 
                                                                                v-model="td.value_field"
                                                                                class="form-control col-12 ip36 p-2"
                                                                                disabled
                                                                            />
                                                                        </div>
                                                                        <div v-if="td.kieu_truong == 'int' || td.kieu_truong == 'float'">
                                                                            <InputNumber
                                                                                spellcheck="false" 
                                                                                v-model="td.value_field" 
                                                                                class="form-control col-12 ip36 p-2"
                                                                                disabled
                                                                            />
                                                                        </div>
                                                                        <div v-if="td.kieu_truong == 'textarea'">
                                                                            <Textarea :max="td.is_length" 
                                                                                spellcheck="false" 
                                                                                v-model="td.value_field" 
                                                                                class="form-control col-12 p-2"
                                                                                disabled
                                                                                rows="1"
                                                                                autoResize
                                                                            />
                                                                        </div>
                                                                        <div v-if="td.kieu_truong == 'switch'">
                                                                            <div class="flex ip36 mb-0" 
                                                                                style="align-items: center; flex-direction: row; justify-content: center;">
                                                                                <InputSwitch v-model="td.value_field" disabled/>
                                                                                <!-- <label class="ml-2">{{ td.ten_truong }}</label> -->
                                                                            </div>
                                                                        </div>
                                                                        <div v-if="td.kieu_truong == 'checkbox'">
                                                                            <div class="flex ip36 mb-0" 
                                                                                style="align-items: center; flex-direction: row; justify-content: center;">
                                                                                <Checkbox v-model="td.value_field" :binary="true" disabled/>
                                                                                <!-- <label class="ml-2">{{ td.ten_truong }}</label> -->
                                                                            </div>
                                                                        </div>
                                                                        <div v-if="td.kieu_truong == 'radio'">
                                                                            <div class="flex ip36 mb-0" 
                                                                                style="align-items: center; flex-direction: row; justify-content: center;">
                                                                                <RadioButton :value="td.request_formd_id" 
                                                                                    v-model="request_data.Radio" disabled/>
                                                                                <!-- <label class="ml-2">{{ td.ten_truong }}</label> -->
                                                                            </div>
                                                                        </div>                                                        
                                                                        <div v-if="td.kieu_truong == 'select' && td.is_type == 9">
                                                                            <Dropdown class="w-full"
                                                                                :options="list_type_dayoff"
                                                                                v-model="td.value_field"
                                                                                optionLabel="name" 
                                                                                optionValue="code" 
                                                                                :placeholder="'-- ' + td.ten_truong + ' --'"
                                                                                style="border:none;"
                                                                                disabled
                                                                            >
                                                                            </Dropdown>
                                                                        </div>
                                                                        <div v-if="td.kieu_truong == 'date' || td.kieu_truong == 'datetime'">
                                                                            <Calendar
                                                                                :showIcon="false"
                                                                                class="form-control col-12 ip36 p-0 calendar-table"
                                                                                autocomplete="on"
                                                                                inputId="time24"
                                                                                v-model="td.value_field"
                                                                                placeholder="dd/mm/yyyy"
                                                                                disabled
                                                                            />
                                                                        </div>
                                                                        <div v-if="td.kieu_truong == 'time'">
                                                                            <Calendar
                                                                                :showIcon="false"
                                                                                class="form-control col-12 ip36 p-0"
                                                                                autocomplete="on"
                                                                                inputId="time24"
                                                                                v-model="td.value_field"
                                                                                placeholder="HH:mm"
                                                                                timeOnly
                                                                                disabled
                                                                            />
                                                                        </div>
                                                                    </div>
                                                                </td>
                                                                <!-- <td class="td-table-render" style="text-align:center">
                                                                    <a v-if="Ftables[idxForm].length > 1" 
                                                                        @click="removeRow(idxForm, indexF)">
                                                                        <i class="pi pi-trash" style="color:red;cursor:pointer;"></i>
                                                                    </a>
                                                                </td> -->
                                                            </tr>
                                                        </tbody>
                                                    </table>
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                                
                                <div class="row mt-2 mb-1">
                                    <div class="t-r">
                                        <div class="flex" style="align-items: center;">
                                            <span class="cv-spicon flex" style="align-items:center;">
                                                <i class="pi pi-file"></i>
                                            </span>
                                            <span class="cv-request">Đề xuất liên quan ({{ RelateRequests.length || 0 }})</span>
                                            <span class="flex ml-2"
                                                v-if="detail_request.IsEdit" 
                                                v-tooltip.top="'Thêm đề xuất liên quan'" 
                                                @click="openRelate(detail_request,'srequest',1)"
                                                style="cursor:pointer;"
                                            >
                                                <i class="pi pi-plus-circle"></i>
                                            </span>
                                        </div>
                                    </div>
                                    <table class="table table-condensed" style="margin-left:0;"
                                        v-if="RelateRequests.length > 0">
                                        <tbody>
                                            <tr v-for="r in RelateRequests">
                                                <td class="" 
                                                    style="cursor:pointer;text-align:center;" 
                                                    @click="openURLRQ(r)" 
                                                    :class="r.status != 2 && r.is_overdue && r.Deadline && r.SoNgayHan <= 24 ? 'overdue-request' : ''"
                                                >
                                                    <span style="word-break: break-all;">{{ r.request_code }}</span>
                                                    <div class="mt-2" v-if="true || r.status_processing == 3">
                                                        <Rating class="star-rating-custom"
                                                            v-model="r.evaluated_score"
                                                            v-tooltip.top="{ value: 'Ngày đánh giá: <br/>' + (r.evaluated_date ? moment(new Date(r.evaluated_date)).format('HH:mm DD/MM/yyyy') : ''), escape: true }"
                                                            :stars="5"
                                                            :cancel="false" 
                                                            :readonly="true"
                                                        />
                                                    </div>
                                                    <span class="rq" 
                                                        :class="r.objStatus.class"
                                                        style="display:flex"
                                                    >
                                                        <strong style="text-align:center;flex:1">{{ r.objStatus.text }}</strong>
                                                        <i class="pi pi-check-circle" v-if="r.status_processsing == 3" style="margin-left:2px"></i>
                                                    </span>
                                                </td>
                                                <td style="cursor:pointer" @click="openURLRQ(r)">
                                                    <span class="uutien"
                                                        :class="'uutien' + (r.priority_level || 0)" 
                                                        v-if="r.priority_level > 0"
                                                    >
                                                        {{ r.priority_level == 1 ? 'Gấp' : 'Rất gấp' }}
                                                    </span>
                                                    <span class="card-nhom flex text-left" 
                                                        style="padding: 0.25rem 0.5rem;background-color: #ff8b4e;color: #fff;margin-right: 0.5rem !important;"
                                                        v-if="r.is_change_process">Quy trình động</span>
                                                    <span style="font-weight:bold">
                                                        {{ r.request_name }}
                                                    </span>
                                                    <p style="font-size:12px;margin:2px 0;line-height: 12px;font-weight:bold;font-weight:500">
                                                        {{ r.full_name }}
                                                    </p>
                                                    <p style="font-size:12px;margin:0;line-height: 12px;">
                                                        {{ r.department_name }}
                                                    </p>
                                                </td>
                                                <td>
                                                    <div class="card-users" 
                                                        @click="showChartSign(r)" 
                                                        style="cursor:pointer;margin:auto;text-align:center"
                                                    >
                                                        <div v-for="(g, idxSign) in limitData(r.Signs, 3)" 
                                                            style="display:inline-block">
                                                            <ul style="margin:0 5px">
                                                                <template v-for="u in g.USign">
                                                                    <li class="IsType0"
                                                                        :class="'IsSign' + u.is_sign + ' ' + 'iclose' + u.is_close + ' ' + 'Trangthai' + u.status"
                                                                        v-tooltip.top="{ value: (u.full_name+'<br/>'+u.position_name+'<br/>'+u.department_name), escape: true }">
                                                                        <Avatar
                                                                            v-bind:label="
                                                                                u.avatar
                                                                                ? ''
                                                                                : (u.last_name_completed_all ?? '').substring(0, 1).toUpperCase()
                                                                            "
                                                                            v-bind:image="
                                                                                u.avatar
                                                                                ? basedomainURL + u.avatar
                                                                                : basedomainURL + '/Portals/Image/nouser1.png'
                                                                            "
                                                                            v-tooltip.top="{ value: (u.full_name+'<br/>'+u.position_name+'<br/>'+u.department_name), escape: true }"
                                                                            style="color: #ffffff; width:2rem; height:2rem; font-size: 1rem !important;"
                                                                            :style="{ background: bgColor[0], }"
                                                                            size="large"
                                                                            shape="circle"
                                                                            class="border-radius"
                                                                        />
                                                                        <span class="IsSign" v-if="u.is_sign && u.is_sign != 0 && u.is_sign != 100">
                                                                            <i :class="'IsSign' + u.is_sign 
                                                                                + ' pi ' + (u.is_sign == 1 ? 'pi-check-circle' :
                                                                                            u.is_sign == -1 ? 'pi-stop-circle' :
                                                                                            u.is_sign == 2 ? 'pi-chevron-circle-right' : '')"
                                                                            ></i>
                                                                        </span>
                                                                    </li>
                                                                </template>
                                                                <template v-if="g.IsTypeDuyet!=0 || g.IsShowTV" v-for="u in limitData(g.Thanhviens,5)">
                                                                    <li
                                                                        class="IsType0"
                                                                        :class="'IsSign' + u.is_sign + ' ' + 'iclose' + u.is_close + ' ' + 'Trangthai' + u.status"
                                                                        v-tooltip.top="{ value: (u.full_name+'<br/>'+u.position_name+'<br/>'+u.department_name), escape: true }"
                                                                    >
                                                                        <Avatar
                                                                            v-bind:label="
                                                                                u.avatar
                                                                                ? ''
                                                                                : (u.last_name_completed_all ?? '').substring(0, 1).toUpperCase()
                                                                            "
                                                                            v-bind:image="
                                                                                u.avatar
                                                                                ? basedomainURL + u.avatar
                                                                                : basedomainURL + '/Portals/Image/nouser1.png'
                                                                            "
                                                                            v-tooltip.top="{ value: (u.full_name+'<br/>'+u.position_name+'<br/>'+u.department_name), escape: true }"
                                                                            style="color: #ffffff; width:2rem; height:2rem; font-size: 1rem !important;"
                                                                            :style="{ background: bgColor[0], }"
                                                                            size="large"
                                                                            shape="circle"
                                                                            class="border-radius"
                                                                        />
                                                                        <span class="IsSign" v-if="u.is_sign && u.is_sign != 0 && u.is_sign != 100">
                                                                            <i :class="'IsSign' + u.is_sign 
                                                                                + ' pi ' + (u.is_sign == 1 ? 'pi-check-circle' :
                                                                                            u.is_sign == -1 ? 'pi-stop-circle' :
                                                                                            u.is_sign == 2 ? 'pi-chevron-circle-right' : '')"
                                                                            ></i>
                                                                        </span>
                                                                    </li>
                                                                </template>
                                                                <li v-if="(g.IsTypeDuyet!=0 || g.IsShowTV) && g.Thanhviens.length > 5" class="IsType1">
                                                                    <div class="divav" 
                                                                        style="background-color:#f8fafb;color:#98a9bc!important;text-align:center;border-radius:50%;padding-top: 2px;margin-left:-16px;border: none;">
                                                                        <span>+{{g.Thanhviens.length - 5}}</span>
                                                                    </div>
                                                                </li>
                                                            </ul>
                                                        </div>
                                                    </div>
                                                </td>
                                                <td style="text-align:center">
                                                    <span>
                                                        {{ (r.created_date ? moment(new Date(r.created_date)).format('DD/MM/yyyy') : '') }}
                                                    </span>
                                                    <div class="mt-2" style="vertical-align:middle;width:120px" v-if="r.Tiendo && r.Tiendo > 0">
                                                        <ProgressBar class="progress-bar-custom" 
                                                            :class="renderColorProgress(r.Tiendo)"
                                                            :value="(r.Tiendo || 0)"
                                                        ></ProgressBar>
                                                    </div>
                                                </td>
                                                <td v-if="detail_request.IsEdit">
                                                    <span style="padding-top:10px;padding-right:5px" 
                                                        @click="Del_Relate('Đề xuất',r.request_relate_id, request_relate)">
                                                        <i class="pi pi-trash"></i>
                                                    </span>
                                                </td>
                                            </tr>
                                        </tbody>
                                    </table>
                                </div>                                
                                
                                <div class="form-group mb-0 mt-3" id="task-file">
                                    <div class="t-r">
                                        <div class="flex" style="align-items: center;">
                                            <span class="cv-spicon">
                                                <i class="pi pi-paperclip"></i>
                                            </span>
                                            <span class="cv-request">
                                                Tài liệu đính kèm ({{ filterFileType(LisFileAttachRQ, 'is_type', 0).length }})
                                            </span>
                                            <span class="flex ml-2"
                                                v-if="detail_request.IsEdit || detail_request.IsComment" 
                                                v-tooltip="'Thêm File đính kèm'"
                                                @click="openModalAddFileCV(detail_request)"
                                                style="margin:5px; cursor:pointer;"
                                            >
                                                 <i class="pi pi-plus-circle"></i>
                                            </span>
                                        </div>
                                    </div>
                                    <div class="row" v-if="filterFileType(LisFileAttachRQ, 'is_type', 0).length>0">
                                        <div class="col-12 ListImagesFile" style="margin:5px 0;cursor:pointer">
                                            <table class="table table-condensed" style="table-layout:fixed;margin-left:0;width:100%;border-spacing: 0;">
                                                <tbody>
                                                    <tr v-for="ffile in filterFileType(LisFileAttachRQ, 'is_type', 0)">
                                                        <td class="td-table-file" width="50" style="text-align: center;">
                                                            <Image v-if="!ffile.is_image"
																class="flex image-type-file"
																:src="basedomainURL + '/Portals/Image/file/' + ffile.file_type + '.png'"
																@error="$event.target.src = basedomainURL + '/Portals/Image/noimg.jpg'"
																style="height: 100%; width: 100%; object-fit: contain;justify-content: center;padding: 0.25rem;"
															/>
                                                            <Image v-else
																class="flex image-type-file"
																:src="basedomainURL + ffile.file_path"
																@error="$event.target.src = basedomainURL + '/Portals/Image/noimg.jpg'"
																style="height: 100%; width: 100%; object-fit: contain;justify-content: center;padding: 0.25rem;"
																preview
															/>
                                                        </td>
                                                        <td class="td-table-file pl-1">
                                                            <a v-if="!ffile.is_image && ffile.file_type != 'pdf'">
                                                                {{ ffile.file_name }}
                                                            </a>
                                                            <a v-if="ffile.file_type == 'pdf'">
                                                                {{ ffile.file_name }}
                                                                <span v-if="ffile.file_path_ca" style="color:green">(Đã thêm chữ ký số)</span>
                                                            </a>
                                                            <a v-if="ffile.is_image">
                                                                {{ ffile.file_name || '' }}
                                                            </a>
                                                        </td>
                                                        <td class="td-table-file" width="120" style="text-align: center;">
                                                            {{ ffile.created_date ? moment(ffile.created_date || new Date()).format("DD/MM/YYYY") : "" }}
                                                        </td>
                                                        <td class="td-table-file" width="100" style="text-align: center;">
                                                            {{ formatByte(ffile.file_size) }}
                                                        </td>
                                                        <td class="td-table-file" 
                                                            :style="detail_request.IsEdit ? 'width:100px' : 'width:50px'"
                                                        >
                                                            <div class="flex" style="justify-content: center;">
                                                                <Button class="p-button-text p-button-secondary p-button-rounded"
                                                                    icon="pi pi-download"
                                                                    @click="downloadFile(ffile)"
                                                                />
                                                                <Button class="p-button-text p-button-danger p-button-rounded ml-1"
                                                                    icon="pi pi-trash"
                                                                    @click="Del_AttachFile(LisFileAttachRQ, ffile)"
                                                                    v-if="detail_request.IsEdit"
                                                                />
                                                            </div>
                                                        </td>
                                                    </tr>
                                                </tbody>
                                            </table>
                                        </div>
                                    </div>
                                </div>                                
                            </form>
                            
                            <div class="t-r mt-3">
                                <div class="flex" style="align-items:center;">
                                    <span class="cv-spicon" style="margin-top:-2px">
                                        <i class="pi pi-comments"></i>
                                    </span>
                                    <span class="cv-request">Thảo luận ({{ Comments.length || 0 }})</span>
                                </div>
                            </div>
                            
                            <div class="task-comment" id="task-comment">
                                <div class="my-3"
                                    style="display:table;height:100%;width:100%;"
                                    v-if="Comments.length == 0"
                                >
                                    <div class="align-items-center justify-c p-0 text-center m-auto">
                                        <img src="../../../../src/assets/background/nodata.png" height="144" />                                        
                                        <h3 class="m-1">Hiện chưa có thảo luận nào cho yêu cầu - đề xuất này.</h3>
                                    </div>
                                </div>
                                
                                <div class="flex" v-for="u in Comments">
                                    <div class="row-comment flex mt-3 mb-1" style="flex:1;">
                                        <div class="r-ava flex pt-2">
                                            <Avatar
                                                v-bind:label="
                                                    u.avatar
                                                    ? ''
                                                    : (u.last_name ?? '').substring(0, 1).toUpperCase()
                                                "
                                                v-bind:image="
                                                    u.avatar
                                                    ? basedomainURL + u.avatar
                                                    : basedomainURL + '/Portals/Image/nouser1.png'
                                                "
                                                v-tooltip.top="{ value: (u.full_name + '<br/>' + u.position_name + '<br/>' + u.department_name), escape: true }"
                                                style="color: #ffffff; font-size: 1rem !important;"
                                                :style="{ background: bgColor[0], }"
                                                size="large"
                                                shape="circle"
                                                class="border-radius"
                                            />
                                        </div>
                                        <div class="r-cbox ml-3 pt-2" style="flex:1;">
                                            <div class="r-cname flex" style="justify-content: space-between;">
                                                <span class="font-bold" style="flex: 1;">{{ u.full_name }} </span>
                                                <span class="r-cdate ml-3">
                                                    {{ (u.created_date ? moment(new Date(u.created_date)).format('HH:mm DD/MM/yyyy') : '') }}
                                                </span>
                                            </div>
                                            <div class="r-cm" style="word-break: break-word;"
                                                v-if="u.content"
                                            >
                                                <div class="reply-chat show-reply"
                                                    style="padding: 10px;border-bottom: 0.5px solid #ccc;margin-bottom: 10px;"
                                                    v-if="u.ParentComment"
                                                >
                                                    <div class="row">
                                                        <div class="content-reply flex">
                                                            <font-awesome-icon icon="fa-solid fa-quote-right" style="font-size: 1rem; color: gray;padding-bottom: 5px;" />
                                                            <div style="display: inline-block; padding: 0px 10px 5px;" 
                                                                class="bind-chat-html" 
                                                                v-html="u.ParentComment.content"
                                                                v-if="u.ParentComment.type_comment == 0">
                                                            </div>
                                                            <div style="display: inline-block; padding: 0px 10px 5px;" class="bind-chat-html" v-else>
																<Image v-if="u.ParentComment.type_comment == 1 && u.ParentComment.files.length > 0"
																	class="flex"
																	:src="basedomainURL + (u.ParentComment.files[0].file_path ||'/Portals/Image/noimg.jpg')"
																	style="height: 3rem; object-fit: contain;"
																/>
																<div class="r-fbox image_file_chat flex" style="align-items: center;" v-else>
																	<img style="width:32px;" 
																		v-bind:src="basedomainURL+'/Portals/Image/file/'+u.ParentComment.files[0].file_type+'.png'" 
																		v-if="u.ParentComment.files.length > 0"
																	/>
																	<a class="ml-2" style="color:#a9a69e; font-size: 0.9rem;" v-if="u.ParentComment.files.length > 0">
																		<b>{{u.ParentComment.files[0].file_name}}</b>
																	</a>
																</div>
															</div>
                                                        </div>
                                                        <div class="name-date-reply" style="text-align: left; white-space: nowrap;">
                                                            <span style="color: #888;font-size: 12px;">
                                                                {{ u.ParentComment.full_name + ', ' + 
																	(moment(new Date(u.ParentComment.created_date)).format("DD/MM/YYYY") == moment(new Date()).format("DD/MM/YYYY") 
																	? ("Hôm nay lúc " + moment(new Date(u.ParentComment.created_date)).format("HH:mm"))
																	: moment(new Date(u.ParentComment.created_date)).format("DD/MM/YYYY"))
																}}
                                                            </span>
                                                        </div>
                                                    </div>
                                                </div>
                                                <a :href="u.content" target="_blank" v-if="u.content.includes('http://') || u.content.includes('https://')">
													<div v-html="u.content" style="display: inline-grid; text-align: -webkit-left; padding: 10px 10px 5px 0; margin-bottom:0;word-break:break-word"></div>
												</a>
												<div v-else v-html="u.content" style="display: inline-grid; text-align: -webkit-left; padding: 10px 10px 5px 0; margin-bottom:0;word-break:break-word"></div>
                                                <!-- <div>
													<span class="r-cdate fw-400">
														{{ u.created_date ? moment(new Date(u.created_date)).format("HH:mm DD/MM") : '' }}
													</span>
												</div> -->
                                            </div>
                                            
                                            <div class="r-file" v-if="u.type_comment != 0 && u.files != null && u.files.length>0" style="padding:0">
                                                <ul>
                                                    <li v-for="f in u.files" class="border-none px-0 py-2">
                                                        <div class="r-fbox image_file_chat">
                                                            <a @click="showfile(f, u)" v-if="f.is_image!='1'">
																<img width="32" v-bind:src="basedomainURL+'/Portals/Image/file/'+f.file_type+'.png'" v-if="f.is_image!='1'" />
															</a>
															<Image v-if="f.is_image=='1'"
																class="flex"
																:src="basedomainURL+f.file_path"
																style="width: 100%; height: 100%; object-fit: contain;"
																preview
															/>
															<a @click="showfile(f, u)" v-if="f.is_image!='1'" style="color:inherit">
																<b>{{f.file_name}}</b>
															</a>
															<a class="viewimg" v-if="f.is_image=='1' || f.file_type=='pdf'" 
																v-bind:href="basedomainURL+f.file_path" 
																data-fancybox :data-caption="f.file_name" 
																style="color:inherit">
															</a>
															<span v-if="f.is_image!='1'">{{formatByte(f.file_size)}}</span>                                                            
                                                        </div>
                                                    </li>
                                                    <!-- <div class="pt-0 pb-3">
														<span class="r-cdate fw-400">
															{{ u.created_date ? moment(new Date(u.created_date)).format("HH:mm DD/MM") : '' }}
														</span>
													</div> -->
                                                </ul>
                                            </div>
                                            <div class="r-action pt-2">
                                                <div class="flex" style="min-width: 100%;">
													<div @click="getStick(u)" style="cursor:pointer" v-if="u.sticks && u.sticks.length > 0">
														<ul class="p-0 mr-2 flex" style="box-shadow: none;list-style: none; margin: 0;">
															<li class="flex" v-for="(st, idxSt) in u.sticks" :key="idxSt" style="border-radius: 50%;">
																<img v-bind:src="basedomainURL+st.stick_file" style="width: 25px;" />
															</li>
															<li class="flex" v-if="u.countstick - 3 > 0" style="border-radius: 50%;">
																<span class="flex pr-1" style="align-items: center;"> +{{u.countstick - 3}}</span>
															</li>
														</ul>
													</div>
													<div class="r-action" style="cursor:pointer" v-if="!u.IsEdit">
														<ul class="p-0 flex" 
                                                            style="background-color: transparent; box-shadow: none;list-style: none; margin: 0;">
															<li class="flex">
																<a v-if="true || u.created_by != store.getters.user.user_id" class="mr-1"
																	@click="showEmote($event, u)">
																	<span class="badge-2 fw-400 m-0" style="font-size: 12px;" v-tooltip.top="'Tương tác'">
                                                                        <span class="flex" style="align-items:center;">
                                                                            <i class="pi pi-thumbs-up"></i>
                                                                            <span class="ml-2">Thích</span>
                                                                        </span>
																	</span>
																</a>
															</li>
															<li class="ml-3 flex">
																<a @click="u.IsReply ? HuyReply() : Reply(u)" 
                                                                    style="margin-right: 5px;"
                                                                    v-if="!detail_request.is_close"
                                                                >
																	<span class="badge-2 fw-400 m-0" style="font-size: 12px;" 
                                                                        v-tooltip.top="u.IsReply?'Hủy':'Trả lời '"
                                                                    >
                                                                        <span class="flex" style="align-items:center;" v-if="u.IsReply">
                                                                            <i class="pi pi-times" style="color:red;"></i>
                                                                            <span class="ml-2">Huỷ</span>
                                                                        </span>
                                                                        <span class="flex" style="align-items:center;" v-else>
                                                                            <i class="pi pi-reply"></i>
                                                                            <span class="ml-2">Trả lời</span>
                                                                        </span>
																		<span style="vertical-align: super;" v-if="u.CountReply != null && u.CountReply.length > 0">
																			{{((u.CountReply.length > 0) ? '('+u.CountReply.length+')' : '') }}
																		</span>
																	</span>
																</a>
															</li>
														</ul>
													</div>
												</div>
                                            </div>
                                            <div v-if="u.IsEdit">
                                                <div class="r-file" v-if="u.FileAttach_Edit.length>0">
                                                    <h3 style="font-weight: bold;font-size: 16px;margin:0;border-top: 1px solid #f5f5f5;padding-top: 15px;color: #2196f3;">
                                                        Danh sách file đã chọn
                                                    </h3>
                                                    <ul style="padding-left: 0; list-style: none;">
                                                        <li class="py-2" v-for="(f, idxFile) in u.FileAttach_Edit">
                                                            <div class="r-fbox flex" style="justify-content: space-between;">
                                                                <div class="flex" style="align-items: center;">
                                                                    <img width="32" v-bind:src="basedomainURL+'/Portals/Image/file/'+f.file_type+'.png'" />
                                                                    <span class="font-bold">{{f.file_name}}</span>
                                                                </div>
                                                                <div class="flex" style="align-items: center;">
                                                                    <span>{{ formatByte(f.file_size) }}</span>
                                                                    <div class="ml-3" style="width:40px;justify-content: center;">
                                                                        <a @click="removeFilesComment(u.FileAttach_Edit, idxFile)">
                                                                            <i style="font-size:20px;color: red;" class="pi pi-times-circle"></i>
                                                                        </a>
                                                                    </div>
                                                                </div>
                                                            </div>
                                                        </li>
                                                    </ul>
                                                </div>
                                            </div>
                                        </div>
                                        
                                        <div class="btn-func-cmt flex ml-2">
                                            <div :id="'menu' + u.request_comment_id">
                                                <Button
                                                    class="p-button-rounded p-button-text p-button-plain p-0"
                                                    icon="pi pi-ellipsis-h"
                                                    @click="toogleFuncMes($event, u, index)"
                                                    aria-haspopup="true"
                                                    aria-controls="overlay_panelFuncMes"
                                                />
                                            </div>
                                        </div>
                                    </div>
                                </div>
                                
                                <div class="r-file" 
                                    style="position: absolute; bottom: 70px; background-color: #fff; width: 100%;" 
                                    v-if="FileAttach.length > 0">
                                    <h3 style="font-weight: bold;font-size: 16px;margin:0;border-top: 1px solid #f5f5f5;padding-top: 15px;color: #2196f3;">
                                        Danh sách file đã chọn
                                    </h3>
                                    <ul class="my-2" style="padding-left: 0; list-style: none;">
                                        <li v-for="(f, idx) in FileAttach" :key="idx">
                                            <div class="r-fbox" style="position: relative;">
                                                <img width="32" v-bind:src="basedomainURL+'/Portals/Image/file/'+f.file_type+'.png'" />
                                                <div class="name-file font-bold py-2" style="word-break:break-all;">
                                                    {{f.file_name}}
                                                </div>
                                                <div>{{ formatByte(f.file_size) }}</div>
                                                <div style="width:40px;position:absolute;top:-10px;right:-20px;">
                                                    <a @click="removeFilesComment(FileAttach,idx)">
                                                        <i style="font-size:20px" class="pi pi-times-circle"></i>
                                                    </a>
                                                </div>
                                            </div>
                                        </li>
                                    </ul>
                                </div>                            
                            </div>
                            
                        </div>
                        <div v-if="detail_request.IsViewXL">
                            <div class="scrollbox-content"
                                style="margin-right:0;"
                            >
                                <div class="box-jobStask">
                                    <template v-for="(job, idxJob) in orderDatas(RQJobs, 'is_order')">
                                        <div class="box-job" v-if="job.isCurrent == true || job.isCurrent == null">
                                            <div class="job-headder">
                                                <a style="padding:5px" data-toggle="collapse" data-target="#CollJob{{job.request_job_id}}">
                                                    <i class="pi pi-angle-down"></i>
                                                </a>
                                                <div class="job-ava" data-toggle="collapse" data-target="#CollJob{{job.request_job_id}}">
                                                    <Avatar
                                                        v-bind:label="
                                                            job.avatar
                                                            ? ''
                                                            : (job.last_name ?? '').substring(0, 1).toUpperCase()
                                                        "
                                                        v-bind:image="
                                                            job.avatar
                                                            ? basedomainURL + job.avatar
                                                            : basedomainURL + '/Portals/Image/nouser1.png'
                                                        "
                                                        v-tooltip.top="{ value: (job.full_name + '<br/>' + job.position_name + '<br/>' + job.department_name), escape: true }"
                                                        style="color: #ffffff; font-size: 1rem !important;"
                                                        :style="{ background: bgColor[0], }"
                                                        size="large"
                                                        shape="circle"
                                                        class="border-radius"
                                                    />
                                                </div>
                                                <div class="job-row" data-toggle="collapse" data-target="#CollJob{{job.request_job_id}}">
                                                    <span class="font-bold">{{ job.job_name }}</span>
                                                    <div class="card-date text-left" style="font-size:12px;flex:1">
                                                        <span>
                                                            {{ (job.start_date ? moment(new Date(job.start_date)).format('HH:mm DD/MM/yyyy') : '') }}
                                                        </span>
                                                        <span class="px-1">-</span>
                                                        <span>
                                                            {{ (job.end_date ? moment(new Date(job.end_date)).format('HH:mm DD/MM/yyyy') : '') }}
                                                        </span>
                                                    </div>
                                                    <div class="duan-process-bg" style="flex:1;position:unset;font-size:11px;padding-top:8px;max-width:200px" v-if="job.Tiendo && job.Tiendo>0">
                                                        <div class="" style="vertical-align:middle;height:16px;">
                                                            <ProgressBar class="progress-bar-custom" 
                                                                :class="renderColorProgress(job.Tiendo)"
                                                                v-tooltip.top="{ value: (job.Tiendo + '%') }" 
                                                                :value="(job.Tiendo || 0)"
                                                                style="flex:1;"
                                                            ></ProgressBar>
                                                        </div>
                                                    </div>
                                                </div>
                                                <div style="height:32px" class="btn-group">
                                                    <button type="button" style="width:110px" 
                                                        :style="{ background: job.TrangthaiColor }"
                                                        class="btn btn-success dropdown-toggle dropdown-toggle-split"
                                                        :class="job.IsFunc && job.status < 2 ? '' : 'no-after'" 
                                                        data-toggle="dropdown">
                                                        <span>{{ job.TrangthaiTen }}</span>
                                                        <span v-if="job.IsFunc && job.status < 2" class="caret"></span>
                                                    </button>
                                                    <div class="dropdown-menu" v-if="job.IsFunc && job.status < 2">
                                                        <a @click="editJob(detail_request,job,2)" class="dropdown-item">
                                                            <i class="pi pi-pencil"></i> Chỉnh sửa nhiệm vụ
                                                        </a>
                                                        <a @click="editJob(detail_request,job,3)" class="dropdown-item">
                                                            <font-awesome-icon class="mr-2" icon="fa-solid fa-list-check" /> Thêm nhanh công việc cho nhiệm vụ
                                                        </a>
                                                        <a @click="openModalAddCV(detail_request,job)" class="dropdown-item">
                                                            <font-awesome-icon class="mr-2" icon="fa-solid fa-list-check" /> Tạo công việc cho nhiệm vụ
                                                        </a>
                                                        <a v-if="idxJob != RQJobs.length-1" @click="MoveJob(idxJob,false)" class="dropdown-item">
                                                            <i class="pi pi-angle-down"></i> Chuyển xuống dưới
                                                        </a>
                                                        <a v-if="idxJob != 0" @click="MoveJob(idxJob,true)" class="dropdown-item">
                                                            <i class="pi pi-angle-up"></i> Chuyển lên
                                                        </a>
                                                        <a @click="DelJob(job)" class="dropdown-item">
                                                            <i class="pi pi-trash"></i> Xóa
                                                        </a>
                                                    </div>
                                                </div>
                                            </div>
                                            <div class="box-task collapse show" id="CollJob{{job.request_job_id}}">
                                                <div class="job-task card" 
                                                    v-for="(d, taskIdx) in orderDatas(job.Congviecs, is_order)" 
                                                    :select="'select' + CongviecID == d.CongviecID"
                                                >
                                                    <div class="card-ten text-left" style="cursor:pointer;-webkit-line-clamp:unset;font-size:14px;flex:1">
                                                        <div style="display:flex">
                                                            <div style="flex:1" @click="goInfoCV(d)">
                                                                {{d.CongviecTen}}
                                                            </div>
                                                            <div style="height:32px" class="btn-group" v-if="job.IsFunc">
                                                                <a class="dropdown-toggle dropdown-toggle-split no-after" data-toggle="dropdown" style="padding:0">
                                                                    <i style="font-size:16px" class="pi pi-ellipsis-h"></i>
                                                                </a>
                                                                <div class="dropdown-menu">
                                                                    <a @click="editJob(detail_request,job,4,d)" class="dropdown-item">
                                                                        <i class="pi pi-pencil"></i> Chỉnh sửa công việc
                                                                    </a>
                                                                    <a v-if="taskIdx != job.ListTasks.length-1" @click="MoveJobTask(job.ListTasks,d,false)" class="dropdown-item">
                                                                        <i class="pi pi-angle-down"></i> Chuyển xuống dưới
                                                                    </a>
                                                                    <a v-if="taskIdx != 0" @click="MoveJobTask(job.ListTasks,d,true)" class="dropdown-item">
                                                                        <i class="pi pi-angle-up"></i> Chuyển lên trên
                                                                    </a>
                                                                    <a @click="DelJobTask(job,d)" class="dropdown-item">
                                                                        <i class="pi pi-trash"></i> Xóa
                                                                    </a>
                                                                </div>
                                                            </div>
                                                        </div>
                                                        <div style="display:flex;">
                                                            <div class="card-date text-left" style="font-size:12px;flex:1">
                                                                <span>
                                                                    {{ (d.start_date ? moment(new Date(d.start_date)).format('DD/MM/yyyy') : '') }}
                                                                </span>
                                                                <span>
                                                                    {{ (d.end_date ? moment(new Date(d.end_date)).format('DD/MM/yyyy') : '') }}
                                                                </span>
                                                            </div>
                                                            <div>
                                                                <span v-if="d.giahan == 0" 
                                                                    style="color:#fff;border-radius:20px;padding:5px 10px;font-size:11px;width:100%;"
                                                                    :style="{ background: d.ttcolor }"
                                                                >
                                                                    {{ d.TrangthaiTen }}
                                                                </span>
                                                                <span v-if="d.giahan > 0" 
                                                                    style="color:#fff;border-radius:20px;padding:5px 10px;font-size:11px;width:100%;background-color:orange"
                                                                >
                                                                    Xin gia hạn
                                                                </span>
                                                            </div>
                                                        </div>
                                                    </div>
                                                    <div style="display:flex" @click="goInfoCV(d)">
                                                        <div class="card-users" style="text-align:left;">
                                                            <ul>
                                                                <template v-for="(u, uIdx) in limitData(d.Thanhviens,5)">
                                                                    <li :class="'IsType' + u.IsType.toString()" 
                                                                        v-tooltip.right="{ value: (u.full_name+'<br/>'+u.position_name+'<br/>'+u.department_name), escape: true }"
                                                                    >
                                                                        <Avatar 
                                                                            v-bind:label="u.avatar ? '' : u.last_name.substring(0, 1)"
                                                                            v-bind:image="basedomainURL + u.avatar"
                                                                            v-tooltip.top="u.full_name"
                                                                            style="
                                                                                background-color: #2196f3;
                                                                                color: #ffffff;
                                                                                width: 3rem;
                                                                                height: 3rem;
                                                                            "
                                                                            :style="{ background: bgColor[uIdx % 7] }"
                                                                            class="text-avatar"
                                                                            size="xlarge"
                                                                            shape="circle"
                                                                        />
                                                                    </li>
                                                                </template>
                                                                <li v-if="d.Thanhviens.length > 5" class="IsType1">
                                                                    <div class="divav" style="background-color:#f8fafb;color:#98a9bc!important;text-align:center;border-radius:50%;padding-top: 2px;margin-left:-16px;border: none;">
                                                                        <span>+{{d.Thanhviens.length-5}}</span>
                                                                    </div>
                                                                </li>
                                                            </ul>
                                                        </div>
                                                        <div class="duan-process-bg" style="flex:1;position:unset;font-size:11px;padding-top:12px;margin:0 10px">
                                                            <div class="" style="vertical-align:middle;" v-if="r.Tiendo && r.Tiendo > 0">
                                                                <ProgressBar class="progress-bar-custom" 
                                                                    :class="renderColorProgress(d.Tiendo)"
                                                                    v-tooltip.top="{ value: (d.Tiendo + '%') }" 
                                                                    :value="(d.Tiendo || 0)"
                                                                    style="flex:1;"
                                                                ></ProgressBar>
                                                            </div>
                                                        </div>
                                                    </div>
                                                </div>
                                            </div>
                                        </div>
                                    </template>
                                </div>
                            </div>
                        </div>
                    </div>                    
                    <div class="flex" v-if="IsReply" style="">
                        <div class="reply-chat show-reply" 
                            style="padding:0;background-color: antiquewhite;border-radius: 10px;margin: 10px 0;width:100%;"
                        >
                            <div class="row">
                                <div class="col-12 md:col-12 flex">
                                    <div class="col-11 content-reply flex">
                                        <font-awesome-icon icon="fa-solid fa-quote-right" style="font-size: 1rem; color: gray;" />
                                        <div style="display: inline-block" class="bind-chat-html ml-2" 
                                            v-html="tinnhanreply.content" 
                                            v-if="tinnhanreply.type_comment == 0"
                                        >
                                        </div>
                                        <div style="display: inline-block" class="bind-chat-html ml-2" v-else>
                                            <Image v-if="tinnhanreply.type_comment == 1 && tinnhanreply.files.length > 0"
                                                class="flex"
                                                :src="basedomainURL + (tinnhanreply.files[0].file_path ||'/Portals/Image/noimg.jpg')"
                                                style="height: 3rem; object-fit: contain;"
                                            />
                                            <div class="r-fbox image_file_chat flex" style="align-items: center;" v-else>
                                                <img style="width:32px;" 
                                                    v-bind:src="basedomainURL+'/Portals/Image/file/'+tinnhanreply.files[0].file_type+'.png'" 
                                                    v-if="tinnhanreply.files.length > 0"
                                                />
                                                <a class="ml-2" style="color:#a9a69e; font-size: 0.9rem;" v-if="tinnhanreply.files.length > 0">
                                                    <b>{{tinnhanreply.files[0].file_name}}</b>
                                                </a>
                                            </div>
                                        </div>
                                    </div>
                                    <div class="col-1 close-reply text-right" v-if="editCmt != true">
                                        <a @click="HuyReply()" style="cursor:pointer;">
                                            <i class="pi pi-times" style="color:red;"></i>
                                        </a>
                                    </div>
                                </div>
                                <div class="col-12 md:col-12 flex name-date-reply">
                                    <span style="padding-left: 10px;color: #888;font-size: 12px;">
                                        {{(tinnhanreply.last_name || tinnhanreply.full_name) + ', ' + 
                                            (moment(new Date(tinnhanreply.created_date)).format("DD/MM/YYYY") == moment(new Date()).format("DD/MM/YYYY") 
                                            ? ("Hôm nay lúc " + moment(new Date(tinnhanreply.created_date)).format("HH:mm"))
                                            : moment(new Date(tinnhanreply.created_date)).format("HH:mm DD/MM/YYYY"))
                                        }}
                                    </span>
                                </div>
                            </div>

                        </div>
                    </div>
                    <div
                        v-if="isClose == false"
                        class="absolute format-center col-12 p-0 m-0 flex bottom-0"
                        style="border-radius: 0.5rem;border: 1px solid #b3b3b3;width:calc(100% - 1.5rem);"
                    >
                        <div class="border-0 flex p-0 m-0" style="flex:1;">
                            <QuillEditor
                                ref="comment_zone_main"
                                placeholder="Nhập nội dung bình luận..."
                                contentType="html"
                                :content="comment"
                                v-model:content="comment"
                                theme="bubble"
                                @selectionChange="Change($event)"
                                style="height: 5rem;width:100%;"
                                @keydown.enter.exact.prevent="addComment()"
                            />
                        </div>
                        <div class="flex p-0 m-0">
                            <div class="format-center flex col-12 p-0 m-0 h-full"
                                v-if="editCmt != true"
                            >
                                <Button
                                    class="p-button-text p-button-plain col-3 format-center w-3rem h-3rem"
                                    @click="showEmoji($event, 1)"
                                    v-tooltip.top="{ value: 'Biểu cảm' }"
                                >
                                    <img alt="logo"
                                        src="/src/assets/image/smile.png"
                                        width="20" height="20"
                                    />
                                </Button>
                                <Button
                                    class="p-button-text p-button-plain col-3 w-3rem h-3rem"
                                    icon="pi pi-paperclip pt-1 pr-0 font-bold"
                                    @click="chooseFile('file_to_request')"
                                    v-tooltip="{ value: 'Đính kèm tệp' }"
                                >
                                </Button>
                                <Button
                                    icon="pi pi-send pt-1 pr-0 font-bold"
                                    class="p-button-text p-button-plain col-3 w-3rem h-3rem"
                                    @click="addComment()"
                                    v-tooltip.top="{ value: 'Gửi bình luận' }"
                                />
                                <input
                                    class="hidden"
                                    id="file_to_request"
                                    type="file"
                                    multiple="true"
                                    accept="*"
                                    @change="PutFileUpload"
                                />
                            </div>
                            <div v-if="editCmt == true" class="showiconchat flex" style="width: 10rem;align-items: center;">
                                <Button class="p-button-text p-button-plain col-3 format-center w-3rem h-3rem"
                                    @click="showEmoji($event, 1)" 
                                    v-tooltip.top="{ value: 'Biểu cảm' }" 
                                >
                                    <img alt="logo"
                                        src="/src/assets/image/smile.png"
                                        width="20" height="20"
                                    />
                                </Button>
                                <Button 
                                    class="p-button-text p-button-plain col-3 w-3rem h-3rem" 
                                    icon="pi pi-send pt-1 pr-0 font-bold"
                                    v-tooltip.top="{ value: 'Gửi bình luận' }"
                                    :disabled="(comment == null || comment == '') && FileAttach.length == 0" 
                                    @click="addComment(0)"
                                >
                                </Button>
                                <Button 
                                    class="p-button-text p-button-plain col-3 w-3rem h-3rem" 
                                    v-tooltip.top="{ value: 'Hủy' }" 
                                    @click="cancelEditComment(cmtFuncClick)"
                                >
                                    <i class="pi pi-times-circle" style="font-size: 1.5rem; color:red;"></i> 
                                </Button>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
        <div class="col-4 md:col-4 pb-0 px-3 pt-3 m-0" v-if="detail_request != null">
            <div class="row col-12 p-0 flex">
                <!-- <div
                    class="col-12 format-center font-bold py-3 mt-3 mb-2 flex"
                    :class="''"
                    style="background-color:#ccf2f6; color: #00626e;"
                >
                    <i class="pi pi-clock pr-2" v-if="TimeToDo != 'Chưa bắt đầu'" />
                    <span class="flex" v-html="TimeToDo"></span>
                </div> -->
                <div class="flex alert-request alert-danger ml-0"
                    style="align-items: center;" 
                    v-if="detail_request.is_overdue"
                >
                    <i class="pi pi-info-circle" style="font-size:16px;margin-top:2px;margin-right:5px"></i>
                    <div class="ml-2">
                        <span class="font-bold" v-if="detail_request.SoNgayHan != 0">Quá hạn ({{ detail_request.SoNgayHan + ' '}} giờ)</span>
                        <span class="font-bold" v-if="detail_request.SoNgayHan == 0">Đến hạn xử lý</span>
                    </div>
                    <span class="font-bold ml-2 span-deadline" v-if="detail_request.status != 2" 
                        @click="openModalDatelineRequest(detail_request)"
                    >
                        Gia hạn
                        <span class="font-bold ml-1" v-if="datelines.length > 0">
                            {{ '(' + datelines.length + ' lần)' }}
                        </span>
                    </span>
                </div>
                <div class="flex alert-request alert-warning ml-0"
                    style="align-items: center;"  
                    v-if="!detail_request.is_overdue && detail_request.times_processing_max > 0 && detail_request.IsProgress"
                >
                    <i class="pi pi-clock" style="font-size:16px"></i>
                    <div class="ml-2">
                        <span class="font-bold" v-if="detail_request.SoNgayHan != 0">Hạn còn {{ detail_request.SoNgayHan || ' ' }} giờ</span>
                        <span class="font-bold" v-if="detail_request.SoNgayHan == 0">Đến hạn xử lý</span>
                    </div>
                    <span class="font-bold ml-2 span-deadline" v-if="detail_request.status != 2" 
                        @click="openModalDatelineRequest(detail_request)"
                    >
                        Gia hạn
                        <span class="ml-1" v-if="datelines.length > 0">
                            {{ '(' + datelines.length + ' lần)' }}
                        </span>
                    </span>
                </div>
                <div class="flex alert-request alert-info ml-0"
                    style="align-items: center;"
                    v-if="!detail_request.is_overdue && !detail_request.IsProgress"
                >
                    <i class="pi pi-clock" style="font-size:16px"></i>
                    <span class="font-bold ml-2">
                        Số giờ xử lý: {{detail_request.times_processing || 0}}/{{detail_request.times_processing_max || 0}} giờ
                    </span>
                </div>
            </div>
            <div class="row col-12 p-0 flex">
                <span class="w-full rq"
                    :class="detail_request.objStatus.class || ''"
                    style="font-size:1rem;font-weight: bold;padding:0.75rem;border-radius:0.25rem;text-align: center;"
                >
                    {{ detail_request.objStatus.text || "" }}
                </span>
                
            </div>
            <div class="row col-12 p-0">
                <TabView class="w-full tab-log-request" lazy :active-index="tabLogActive" @tab-change="changeTabContent">
                    <TabPanel>
                        <template #header>
                            <i class="pi pi-chart-line mr-2 css-icon"></i>
                            <span>QT</span>
                        </template>
                        <div class="" v-if="true">
                            <div v-for="(item, qtIndex) in dataQT"
                                :class="{ 'is-close': item.is_close }"
                                :key="qtIndex"
                                class="bg-blue-200 mb-3"
                            >
                                <Panel :toggleable="true"
                                    :collapsed="item.is_close"
                                >
                                    <template #header>
                                        <div class="flex">
                                            <h3 class="m-0 format-flex-center"
                                                style="text-align: left"
                                            >
                                                {{ item.config_process_name }}
                                            </h3>
                                            <Tag v-if="item.type_send != 2"
                                                class="ml-3 px-3 py-1"
                                                :value="item.type_name"
                                                :class="'type' + item.config_process_type"
                                                style="font-size: 11px; min-width: max-content; color: #fff; 
                                                    border-radius: 25px; height: max-content;"
                                            ></Tag>
                                            <Tag
                                                v-if="item.is_close"
                                                class="mx-3 px-3 py-1"
                                                :value="'Đã hủy'"
                                                :style="{ backgroundColor: 'red', color: '#fff' }"
                                                style="font-size: 11px; min-width: max-content; height: max-content;"
                                            ></Tag>
                                        </div>
                                    </template>
                                    <div v-if="item.approves_groups.length > 0">
                                        <div class=" mt-3"
                                            v-for="(sign, sindex) in item.approves_groups"
                                            :key="sindex"
                                        >
                                            <div class="p-2 flex"
                                                style="background-color: antiquewhite; justify-content: space-between;"
                                            >
                                                <h3 class="m-0 format-flex-center">
                                                    {{ sign.approved_group_name }} ({{ sign.hrm_process.length }} người)
                                                </h3>
                                                <Tag
                                                    class="ml-3 px-3 py-1"
                                                    :value="sign.type_name"
                                                    :class="'type' + sign.approved_type"
                                                    style="font-size: 11px; min-width: max-content; color: #fff; 
                                                        border-radius: 25px; height: max-content;"
                                                ></Tag>
                                            </div>
                                            <div v-if="sign.hrm_process.length > 0">
                                                <div v-for="(signuser, uindex) in sign.hrm_process"
                                                    :key="uindex"
                                                    class="flex mt-3"
                                                    :class="'is-sign' + signuser.is_approved + ' status-sign' + signuser.is_returned"
                                                >
                                                    <div class="signuser-image">
                                                        <div class="group-sign">
                                                            <div style="display: inline-block; position: relative; z-index: 1;"
                                                            >
                                                                <Avatar 
                                                                    v-bind:label="signuser.avatar ? '' : signuser.last_name.substring(0, 1)"
                                                                    v-bind:image="basedomainURL + signuser.avatar"
                                                                    v-tooltip.top="signuser.full_name"
                                                                    style="
                                                                        background-color: #2196f3;
                                                                        color: #ffffff;
                                                                        width: 3rem;
                                                                        height: 3rem;
                                                                    "
                                                                    :style="{ background: bgColor[sindex % 7] }"
                                                                    class="text-avatar"
                                                                    size="xlarge"
                                                                    shape="circle"
                                                                />
                                                                <span class="is-sign">                                                                
                                                                    <font-awesome-icon
                                                                        v-if="signuser.is_approved === '1'"
                                                                        icon="fa-solid fa-circle-check"
                                                                        style="
                                                                        font-size: 16px;
                                                                        display: block;
                                                                        color: #7abd1a;
                                                                        "
                                                                    />
                                                                    <font-awesome-icon
                                                                        v-if="signuser.is_returned === '1'"
                                                                        icon="fa-solid fa-circle-stop"
                                                                        style="
                                                                        font-size: 16px;
                                                                        display: block;
                                                                        color: #ff8b4e;
                                                                        "
                                                                    />
                                                                </span>
                                                            </div>
                                                        </div>
                                                        <span class="sign-date"
                                                            v-if="signuser.date_approved != null"
                                                        >{{ signuser.date_approved }}
                                                        </span>
                                                    </div>
                                                    <div class="signuser-detail"
                                                        :class="{ 'signuser-last': uindex === sign.hrm_process.length - 1 }"
                                                    >
                                                        <div>
                                                            <h3 class="m-0 mb-2">
                                                                {{ signuser.full_name }}
                                                            </h3>
                                                            <div class="description">
                                                                <div>{{ signuser.position_name }}</div>
                                                                <div>{{ signuser.department_name }}</div>
                                                            </div>
                                                            <div class="mt-2"
                                                                v-if="signuser.content != null"
                                                            >
                                                                <span v-if="sign.approved_type === 0">Trình duyệt: </span>
                                                                <span v-else-if="signuser.is_approved === '1'">Chấp thuận: </span>
                                                                <span v-else-if="signuser.is_returned === '1'">Trả lại: </span>
                                                                <!-- <span v-if="signuser.is_type === 0 && signuser.is_sign == -3">Hủy lịch: </span> -->
                                                                <span>{{ signuser.content }}</span>
                                                            </div>
                                                            <div v-if="signuser.files && signuser.files.length > 0"
                                                                class="mt-2 description"
                                                            >
                                                                <a class="hover"
                                                                    @click="showFile(signuser.files[0])"
                                                                >
                                                                    Tài liệu đính kèm
                                                                    <i class="pi pi-paperclip"></i>
                                                                </a>
                                                            </div>
                                                        </div>
                                                    </div>
                                                </div>
                                            </div>
                                            <div v-else
                                                style="width: 100%; height: 100px"
                                                class="format-flex-center"
                                            >
                                                <span class="description">Không có người duyệt</span>
                                            </div>
                                        </div>
                                    </div>
                                    <div v-else
                                        style="width: 100%; height: 100px"
                                        class="format-flex-center"
                                    >
                                        <span class="description">Không có nhóm duyệt</span>
                                    </div>
                                </Panel>
                            </div>
                        </div>
                        <div class="align-items-center justify-content-center p-4 text-center" v-else>
                            <img src="../../../assets/background/nodata.png" height="144" />
                            <h3 class="m-1">Không có dữ liệu</h3>
                        </div>
                    </TabPanel>
                    <TabPanel>
                        <template #header>
                            <i class="pi pi-clock mr-2 css-icon"></i>
                            <span>Log</span>
                        </template>
                        <div class="" v-if="true">
                            <div v-for="(item, logIndex) in dataLog"
                                :class="{ 'is-close': item.is_close }"
                                :key="logIndex"
                                class="bg-blue-200 mb-3"
                            >
                                <Panel :toggleable="true"
                                    :collapsed="item.is_close"
                                >
                                    <template #header>
                                        <span>{{ 'Log 1' }}</span>
                                    </template>
                                    <div>

                                    </div>
                                </Panel>
                            </div>
                        </div>
                        <div class="align-items-center justify-content-center p-4 text-center" v-else>
                            <img src="../../../assets/background/nodata.png" height="144" />
                            <h3 class="m-1">Không có dữ liệu</h3>
                        </div>
                    </TabPanel>
                    <TabPanel>
                        <template #header>
                            <font-awesome-icon class="mr-2" icon="fa-solid fa-list-check" />
                            <span>Công việc</span>
                        </template>
                        <div class="" v-if="true">
                            <Accordion class="accordion-custom tab-law" 
                                :activeIndex="0"
                                :multiple="true"
                            >
                                <AccordionTab>
                                    <template #header>
                                        <font-awesome-icon class="mr-2" icon="fa-solid fa-list-check" />
                                        <span>Công việc (0)</span>
                                    </template>
                                    <div>

                                    </div>
                                </AccordionTab>
                                <AccordionTab>
                                    <template #header>
                                        <i class="pi pi-file mr-2 css-icon"></i>
                                        <span>Văn bản (0)</span>
                                    </template>
                                    <div>
                                        
                                    </div>
                                </AccordionTab>
                                <AccordionTab>
                                    <template #header>
                                        <i class="pi pi-calendar mr-2 css-icon"></i>
                                        <span>Lịch (0)</span>
                                    </template>
                                    <div>
                                        
                                    </div>
                                </AccordionTab>
                                <AccordionTab>
                                    <template #header>
                                        <i class="pi pi-paperclip mr-2 css-icon"></i>
                                        <span>Danh sách tài liệu (0)</span>
                                    </template>
                                    <div>
                                        
                                    </div>
                                </AccordionTab>
                            </Accordion>
                        </div>
                        <div class="align-items-center justify-content-center p-4 text-center" v-else>
                            <img src="../../../assets/background/nodata.png" height="144" />
                            <h3 class="m-1">Không có dữ liệu</h3>
                        </div>
                    </TabPanel>
                    <TabPanel>
                        <template #header>
                            <i class="pi pi-user mr-2 css-icon"></i>
                            <span>Theo dõi</span>
                        </template>
                        <div class="" v-if="true">
                        
                        </div>
                        <div class="align-items-center justify-content-center p-4 text-center" v-else>
                            <img src="../../../assets/background/nodata.png" height="144" />
                            <h3 class="m-1">Không có dữ liệu</h3>
                        </div>
                    </TabPanel>
                </TabView>
            </div>
        </div>
    </div>
    <div class="overflow-hidden w-full"
        style="
            display: grid;
            align-content: center;
            justify-content: center;
            align-items: center;
            justify-items: center;
            height: 98vh;
        "
        v-else
    >
        <img
            src="../../../assets/background/nodata.png"
            height="250"
        />
        <h2 class="m-1">Đề xuất bảo mật, đã bị xóa hoặc không tồn tại.</h2>
    </div>
    <!-- OverlayPanel Emoji -->
    <OverlayPanel
        class="p-0"
        ref="panelEmoij1"
        append-to="body"
        :show-close-icon="false"
        id="overlay_panelEmoij1"
        style="z-index:10000;"
    >
        <VuemojiPicker @emojiClick="handleEmojiClick" />
    </OverlayPanel>
    <!-- panel menu comment -->
    <OverlayPanel
        ref="funcCmtRequest"
        appendTo="body"
        class="p-0 m-0 panelFuncMes"
        :showCloseIcon="false"
        id="overlay_panelFuncMes"
        style="width: fit-content;z-index:10000;"
    >
        <div>
            <ul class="ul-func-mes m-0" style="width: 10rem;">
                <li class="px-2 py-2" v-if="true || (cmtFuncClick.IsMe && cmtFuncClick.type_comment == 0)">
                    <a @click="EditComment(cmtFuncClick)" class="d-b td-n">
                        <i class="pi pi-pencil"></i>
                        <span class="ml-1"> Chỉnh sửa</span>
                    </a>
                </li>
                <li class="px-2 py-2"> <a @click="Reply(cmtFuncClick)" class="d-b td-n">
                    <i class="pi pi-reply"></i>
                    <span class="ml-1"> Trả lời</span></a>
                </li>
                <li class="px-2 py-2" v-if="true || (cmtFuncClick.type_comment == 1 || cmtFuncClick.type_comment == 2 || cmtFuncClick.type_comment == 3 || cmtFuncClick.type_comment == 4)"> 
                    <a @click="downloadFile(cmtFuncClick)" class="d-b td-n">
                    <i class="pi pi-download"></i>
                    <span class="ml-1"> Tải xuống</span></a>
                </li>
                <li v-if="true || cmtFuncClick.IsMe" role="separator" class="divider"></li>
                <li class="px-2 py-2" v-if="true || (detail_request.isdelcomment || cmtFuncClick.IsMe)"> 
                    <a @click="Del_Comment(cmtFuncClick,cmtFuncClick.index)" class="d-b td-n" style="color:red;">
                        <i class="pi pi-trash"></i> 
                        <span class="ml-1"> Xóa</span>
                    </a>
                </li>
            </ul>
        </div>
    </OverlayPanel>
    <!-- panel list emote -->
    <OverlayPanel
        class="p-0 p-overlaypanel-setup"
        ref="panelEmote"
        appendTo="body"
        :showCloseIcon="false"
        id="overlay_panelEmote"
        style="z-index:10000;"
    >
        <div class="flex p-0">
            <div class="p-0 cursor-pointer format-center"
                style="width: 40px; height: 40px"					
                @mouseover="onCheckHover(item)"
                @mouseleave="hideCheckHover()"
                v-for="(item, index) in emoteList" :key="index"
            >
                <img v-tooltip.top="item.emote_name"
                    :src="basedomainURL + item.emote_file"
                    :alt="item.emote_file"
                    @click="addEmote(item)"
                    :class="checkHover == item.emote_id
                        ? 'animate__animated  animate__pulse emote-lg'
                        : 'emote-md'
                    "
                />
            </div>
        </div>
    </OverlayPanel>
    <!-- panel button download file -->
    <OverlayPanel
        ref="funcMesFiles"
        appendTo="body"
        class="p-0 m-0 panelFuncFiles"
        :showCloseIcon="false"
        id="overlay_panelFuncFiles"
        style="width: fit-content;z-index:10000;"
    >
        <div>
            <ul class="ul-func-mes m-0" style="width: 10rem; padding:0; list-style-type: none;">
                <li class="px-2 py-2">
                    <a @click="downloadFile(fileFuncClick)" class="d-b td-n">
                        <i class="pi pi-download"></i>
                        <span class="ml-1"> Tải xuống</span>
                    </a>
                </li>
            </ul>
        </div>
    </OverlayPanel>
    <!-- view file -->
    <Sidebar v-model:visible="displayModalIframeReq" 
        position="full" style="z-index:1001;">
        <iframe style="height: calc(100vh - 3.3rem)" 
            :src="basedomainURL + '/Viewer?title=' + fileShow.file_name + '&url=' + fileShow.file_path" 
            class="w-full" v-if="typeShow == 2">
        </iframe>
        <video autoplay muted controls style="width: -webkit-fill-available; height: -webkit-fill-available;" v-if="typeShow == 3">
            <source :src="basedomainURL + fileShow.file_path" >
        </video>
        <audio autoplay controls style="width:50%;" v-if="typeShow == 4">
            <source :src="basedomainURL + fileShow.file_path">
        </audio>
    </Sidebar>
</template>
<style scoped>
    @import url(../style_request.css);
</style>
<style lang="scss" scoped>
    ::v-deep(.classOver0) {
        .p-progressbar-value {
            background: #ff0000;
        }
    }
    ::v-deep(.classOver30) {
        .p-progressbar-value {
            background: #fe4d97;
        }
    }
    ::v-deep(.classOver50) {
        .p-progressbar-value {
            background: #2196f3;
        }
    }
    ::v-deep(.classOver75) {
        .p-progressbar-value {
            background: #6dd230;
        }
    }
    ::v-deep(.image-type-file) {
        img {
            max-height: 32px; 
            max-width: 100%; 
            object-fit: contain;
        }
    }
    ::v-deep(.tab-log-request) {
        .p-tabview-nav-link:hover {
            background-color: #f0f8ff !important;
        }
        .p-tabview-panels {
            padding: 0.5rem 0;
        }
        .p-accordion-tab {
            padding-bottom: 0.5rem;
        }
        .p-accordion-header {
            border: 1px solid #efefef;
            border-radius: 5px;
        }
        .p-accordion-header.p-highlight .p-accordion-header-link {
            background-color: aliceblue;
        }
        .p-accordion-content {
            border: none;
        }
    }
</style>