<script setup>
import { ref, inject, onMounted, watch } from "vue";
import { useToast } from "vue-toastification";
import { required, maxLength } from "@vuelidate/validators";
import { useVuelidate } from "@vuelidate/core";
import moment from "moment";
import { useCookies } from "vue3-cookies";
import { VuemojiPicker } from "vuemoji-picker";
//import { forEach } from "jszip";
//import DetailBoxChat from "../../components/chat/BoxChat.vue";
import { encr, change_unsigned } from "../../util/function.js";
const cryoptojs = inject("cryptojs");
//const emitter = inject("emitter"); 
const toast = useToast();
//const router = inject("router");
const axios = inject("axios");
const store = inject("store");
const swal = inject("$swal");
const socket = inject("socket");
// reload component
//const componentKeyChat = ref(0);
// const forceRerenderChat = () => {
// 	componentKeyChat.value += 1;
// };
const { cookies } = useCookies();
const basedomainURL = baseURL;
const baseUrlCheck = baseURL;
//const isCheckLaw = ref(false);
const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};
const props = defineProps({
	key: Number,
	detailChat: Object,
	listMessage: Object,
	listMember: Object,
	listGroupChat: Object,
	refreshData: Function,
	funcCallSocket: Function,
	funcCallUpdate: Function,
});
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
const bgColor = ref([
    "#F8E69A", "#AFDFCF", "#F4B2A3", "#9A97EC", "#CAE2B0", "#8BCFFB", "#CCADD7"
]);
const chat = ref({
	chat_group_id: null,
	chat_group_name: '',
	status: 1,
	organization_id: store.getters.user.organization_id,
	searchU: null,
});
const rules = {
  chat_group_name: {
    required,
	maxLength: maxLength(500),
    $errors: [
		{
			$property: "chat_group_name",
			$validator: "required",
			$message: "Tên cuộc trò chuyện không được để trống!",
		},
    ],
  },
};
const v$ = useVuelidate(rules, chat);
const DataDichvu = ref({
	IsUse: true,
});
const IsCall = ref(false);
const FileAttach = ref([]);
const tailieus = ref([]);
const listFileTailieu = () => {
	axios
		.post(
			baseUrlCheck + "api/chat/GetDataProc",
			{ 
				str: encr(JSON.stringify({
						proc: "chat_get_files",
						par: [
							{ par: "chat_group_id", va: props.detailChat.chat_group_id },
							{ par: "user_id ", va: store.getters.user.user_id },
						],
					}), SecretKey, cryoptojs
				).toString()
			},
			config
		)
		.then((response) => {
			let data = JSON.parse(response.data.data);
			tailieus.value = data;
			listActiveTabInfoChat.value = [4];
			if (tailieus.value[0].length > 0){
				listActiveTabInfoChat.value.push(0);
			}
			if (tailieus.value[1].length > 0){
				listActiveTabInfoChat.value.push(1);
			}
			if (tailieus.value[2].length > 0){
				listActiveTabInfoChat.value.push(2);
			}
			if (tailieus.value[3].length > 0){
				listActiveTabInfoChat.value.push(3);
			}
		})
		.catch((error) => {
			//toast.error("Tải dữ liệu không thành công!");
			console.log("Err c_g_fs");
			if (error && error.status === 401) {
				swal.fire({					
					text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",					
					confirmButtonText: "OK",
				});
				store.commit("gologout");
			}
		});
};
const IsReply = ref(false);
const noiDungChat = ref({
	noiDung: "",
});
const completecm = (event) => {
	var evt = event;
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
			url: baseUrlCheck + `/api/chat/ScanFileUpload`,
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
				//console.log(response.data.ms);
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
			goBottomChat();
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
const setdropZone = (idk) => {
	var dropZone = document.getElementById(idk);
	if (dropZone) {
		dropZone.ondrop = (e) => {
			e.preventDefault();
			//this.className = 'upload-drop-zone-1';
			PutFileUpload(e.dataTransfer.files, 'dropFiles');
		};
		dropZone.ondragover = () => {
			//this.className = 'upload-drop-zone-1 drop';
			return false;
		};
		dropZone.ondragleave = () => {
			//this.className = 'upload-drop-zone-1';
			return false;
		};
	}	
};
const bottomChat = ref(0);
const heightFinalChat = ref(0);
const goBottomChat = (type) => {
	setTimeout(() => {
		let elmnt = document.getElementById("chat_final");
		if (elmnt != null) {
  			//elmnt.scrollIntoView({ behavior: "smooth" });
  			elmnt.scrollIntoView();
			bottomChat.value = document.getElementById("task-comment").scrollTop;
			heightFinalChat.value = document.getElementById("chat_final").scrollHeight;
		}
	}, 200);
};

const removeFilesComment = (files, i) => {
	files.splice(i, 1);
};
const loadding = ref(false);
const ReplyID = ref();
const ThongKeChung = ref({
	SoMessage: 0
});
const tinnhanreply = ref({});
const HuyReply = () => {
	var co = props.listMessage.find(x => x.chat_message_id === ReplyID.value);
	IsReply.value = false;
	ReplyID.value = null;
	tinnhanreply.value = null;
	co.IsReply = false;
	FileAttach.value = [];
};
const Reply = (co) => {
	props.listMessage.filter(x => x.IsReply).forEach((r, idx) => {
		r.IsReply = false;
	});
	IsReply.value = true;
	ReplyID.value = co.chat_message_id;
	co.IsReply = true;
	tinnhanreply.value = co;
};
const sendMS = (loai, mesChat) => {
	if (loadding.value) {
		return false;
	}
	if (!noiDungChat.value.noiDung && (FileAttach.value == null || FileAttach.value.length === 0)) {
		swal.fire({
			type: 'error',
			icon: 'error',
			title: '',
			text: 'Vui lòng nhập nội dung chat !'
		});
		return false;
	}
	var ms_copy = {};
	if (mesChat == null || (mesChat != null && mesChat.chat_group_id == null)) {
		var ms = { chat_message_id: null, 
			chat_parent_id: ReplyID.value, chat_group_id: props.detailChat.chat_group_id || null, 
			created_by: store.getters.user.user_id, full_name: store.getters.user.full_name, 
			content_message: noiDungChat.value.noiDung.replaceAll("\n", "<br/>"), 
			status: 0, type_message: loai || 0, IsType: 0, IsMe: true };
		//ms.files = FileAttach.value;
		ms_copy = { ...ms };
	}
	else {
		mesChat.chat_parent_id = mesChat.chat_parent_id == null ? ReplyID.value : mesChat.chat_parent_id;
		mesChat.content_message = mesChat.noiDung;
		ms_copy = JSON.parse(JSON.stringify(mesChat));
	}
	noiDungChat.value = { noiDung: "" };
	
	var formData = new FormData();
	formData.append("models", JSON.stringify(ms_copy));
	//Progress File
	var hasfile = FileAttach.value && FileAttach.value.length > 0;
	if (!hasfile) {
		//swal.showLoading();
	} else {
		FileAttach.value.forEach((file, i) => {
			formData.append('fileUp', file);
		});
	}
	
	loadding.value = true;
	axios({
		method: ms_copy.chat_message_id == null ? 'post' : 'put',
		url: baseUrlCheck +
			`/api/chat/${ms_copy.chat_message_id == null ? "Add_Message" : "Update_Message"}`,
		data: formData,
		headers: {
			Authorization: `Bearer ${store.getters.token}`,
		},
	})
	.then((response) => {
		//if (!hasfile) swal.close();
		//ThongKeChung.value.SoMessage += 1;		
		if (response.data.err !== '0') {
			swal.fire({
				icon: 'error',
				type: 'error',
				title: 'Thông báo',
				text: 'Có lỗi khi thêm tin nhắn mới !'
			});
			return false;
		}
		
		var UpFile = false;
		if (FileAttach.value != null && FileAttach.value.length > 0) {
			UpFile = true;
			listFileTailieu();
		}
		FileAttach.value = [];
		//$('input[name=FileAttachChat]').val("");
		if (ReplyID.value) HuyReply();
		// emitter.emit("emitData", {
		// 	type: "loadListChatGroup",
		// 	data:  null
		// });
		props.funcCallUpdate();
		goBottomChat();
		if (response.data.mess.length > 0) {
			response.data.mess.forEach((message, idx) => {
				let title = "";
				let listSendTo = [];
				if (props.detailChat.is_group_chat) {
					title = ("vừa gửi tin nhắn đến nhóm chat '" + props.detailChat.chat_group_name + "'!");
					let listReceiver = props.listMember.map((x) => x.user_join);
					listSendTo = listSendTo.concat(listReceiver);
				}
				else {
					title = "vửa gửi cho bạn một tin nhắn!";
					let userReceive = props.detailChat.user_chat != store.getters.user.user_id ? props.detailChat.user_chat : props.detailChat.created_by;
					listSendTo.push(userReceive);
				}
				let mes = {
					//app
					uuid: message.chat_message_id,
					chat_group_id: ms.chat_group_id,
					user_id: store.getters.user.user_id,
					sender: store.getters.user.user_id,
					content_message: ms.content_message,
					type_message: message.type_message,
					date_send: moment(ms.created_date).format('YYYY-MM-DDTHH:mm:ss'),
					fullName: store.getters.user.full_name,
					avatar: store.getters.user.avatar != null ? store.getters.user.avatar.replace(basedomainURL,'') : "",
					status: 0,
					isAdd: true,
					event: "getSendMessage",
					socketid: socket.id,
					chat_message_id: message.chat_message_id,
					chat_parent_id: message.chat_parent_id,
					//file_name: message.file_name,
					//file_type: message.file_type,
					//file_path: message.file_path,					
					last_name: store.getters.user.last_name ? store.getters.user.last_name 
								: (store.getters.user.full_name != null ? store.getters.user.full_name.substring(store.getters.user.full_name.trim().lastIndexOf(' ') + 1) : 'anonymous'),
					Title: title,
					UpFile: UpFile,
					to_user_id: listSendTo[0],
					uids: listSendTo,
					title_chat: props.detailChat.chat_group_name,
				}
				//socket.emit('sendData', mes);
				props.funcCallSocket(mes);
			});
		}
	})
	.catch((error) => {
		//toast.error("Tải dữ liệu không thành công!");
		//console.log("Xảy ra lỗi khi lưu tin nhắn");
		if (error && error.status === 401) {
			swal.fire({
				text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
				confirmButtonText: "OK",
			});
			store.commit("gologout");
		}
	});	
};
const changeContent = (ev) => {
	if (ev.keyCode == 13 && !ev.shiftKey)
	{
		ev.preventDefault();
	}
}
const headerChat = ref();
const addUser = ref();
const displayChat = ref(false);
const filterUserToGroup = ref([]);

const typeGroupChat = ref(1);
const Edit_GroupChat = (gr) => {
	if (!gr.is_group_chat) {
		return false;
	}
	props.listGroupChat.forEach((c, idx) => {
		c.active = false;
	});
	props.listGroupChat.find(x => x.chat_group_id === gr.chat_group_id).active = true;
	if (!gr || gr.chat_group_id == null) {
		swal.fire({
			icon: 'error',
			type: 'error',
			title: '',
			text: 'Bạn chưa chọn nhóm chat!'
		});
		return false;
	}
	addUser.value = true;
	if (gr.IsMe) {
		addUser.value = false;
	}
	
	axios
		.post(
			baseUrlCheck + "api/chat/GetDataProc",
			{ 
				str: encr(JSON.stringify({
						proc: "chat_get_groupchat",
						par: [
							{ "par": "chat_group_id", "va": gr.chat_group_id },
							{ "par": "user_id ", "va": store.getters.user.user_id },
						],
					}), SecretKey, cryoptojs
				).toString()
			},
			config
		)
		.then((response) => {
			if (response.data.err != '1') {
				let data = JSON.parse(response.data.data);
				chat.value = data[0][0];
				chat.value.active = true;
				//chat.value.IsInfoChat = localStorage.getItem("viewTabChatID") != null ? localStorage.getItem("viewTabChatID") == 'true': props.detailChat.IsInfoChat;
				chat.value.IsInfoChat = cookies.get("viewTabChatID") != null ? cookies.get("viewTabChatID") == 'true': props.detailChat.IsInfoChat;
				/*
				chat.value.members = [];
				chat.value.countmb = 0;
				if (data[1] && data[1].length > 0) {
					chat.value.members = data[1];
					if (chat.value.members && chat.value.members.length > 0) {
						chat.value.members.forEach((mb, idx) => {
							var i = filterUserToGroup.value.findIndex(x => x.user_id === mb.user_join);
							if (i !== -1) {
								filterUserToGroup.value.splice(i, 1);
							}
							else {
								let dataOneline = filterUser.value.find(x => x.user_id === mb.user_join);
								if (dataOneline) {
									mb.Online = dataOneline.Online || false;
									mb.lastOnline = dataOneline != null ? dataOneline.lastOnline : null;
									if (mb.lastOnline) mb.lastOnline = new Date(mb.lastOnline);
								}
							}
						});
						props.listMember = [...chat.value.members];
					}
				}
				if (chat.value.is_group_chat) {
					chat.value.countmb = data[1].length;
				}
				*/
				headerChat.value = addUser.value ? "Thêm thành viên" : "Cập nhật nhóm chat";
				displayChat.value = true;
				typeGroupChat.value = chat.value.is_group_chat || false;
			}
		})
		.catch((error) => {
			//console.log(error);
			//toast.error("Tải dữ liệu theo dõi không thành công!");
			if (error && error.status === 401) {
				swal.fire({					
					text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",					
					confirmButtonText: "OK",
				});
				store.commit("gologout");
			}
		});
};
const chooseImage = (id) => {
  	document.getElementById(id).value = "";
  	document.getElementById(id).click();
};
let files = [];
const isDisplayAvt = ref(false);
const handleFileUpload = (event) => {
	isDisplayAvt.value = true;
	files = event.target.files;
	var output = document.getElementById("groupChatAvt");
	output.src = URL.createObjectURL(event.target.files[0]);
	output.onload = function () {
		URL.revokeObjectURL(output.src); // free memory
	};
};
const delAvatar = () => {
	files = [];
	isDisplayAvt.value = false;
	var output = document.getElementById("groupChatAvt");
	output.src = basedomainURL + "/Portals/Image/image_group_user.jpg";
	chat.value.avatar_group = null;
};
const removeUser = (us, idx) => {
	if (us[idx].chat_member_id) {
		Out_GroupChat(chat.value, 3, us[idx].user_join, store.getters.user.user_id);
	}
	else {
		if (filterUserCopy.value.filter(x => x.user_id == us[idx].user_id).length == 0){
			filterUserCopy.value.push(us[idx]);
			filterUserCopy.value = filterUserCopy.value.sort((a, b) => a.full_name - b.full_name);
		}
		us.splice(idx, 1);
	}
};

const filterUser = ref([]);
const filterUserCopy = ref([]);

const filterListUser = (pb) => {
	var output = [];
	if (chat.value.searchU != null && chat.value.searchU.trim() != "") {
		pb.listUserPB.filter(x => x.user_id !== store.getters.user.user_id).forEach((u, idx) => {
			let keySearchUser = change_unsigned(chat.value.searchU);
			if (change_unsigned(u.full_name || "").indexOf(keySearchUser) >= 0 || u.user_id.indexOf(keySearchUser) >= 0) {
				if (!chat.value || chat.value.user_id !== u.user_id) {
					u.user_join = u.user_id;
					u.is_order = idx;
					output.push(u);
				}
				u.is_order = idx;
			}
			u.is_order = idx;
		});
		if (props.listMember != null && props.listMember.length > 0) {
			props.listMember.forEach((item, idx) => {
				var i = output.findIndex(x => x.user_join === item.user_join);
				if (i !== -1) {
					output.splice(i, 1);
				}
			});
		}
		return output;
	}
	else {
		pb.listUserPB.filter(x => x.user_id !== store.getters.user.user_id).forEach((u, idx) => {
			u.user_join = u.user_id;
			u.is_order = idx;
			output.push(u);
		});
		return output;
	}
};
const departments = ref([]);
const listDepartmentsUser = () => {
	axios
		.post(
			baseUrlCheck + "api/chat/GetDataProc",
			{ 
				str: encr(JSON.stringify({
						proc: "chat_departments_list_tree",
						par: [
							{ par: "organization_id", va: store.getters.user.organization_id },
							{ par: "user_id ", va: store.getters.user.user_id },
						],
					}), SecretKey, cryoptojs
				).toString()
			},
			config
		)
		.then((response) => {
			let data = JSON.parse(response.data.data);
			data[1].forEach((el, idx) => {
				el.is_order = idx;
			});
			filterUser.value = data[1];
			filterUserCopy.value = data[1];
			data[0].forEach((el, idx) => {
				el.listUserPB = filterUser.value.filter(x => x.department_id == el.department_id && x.user_id != store.getters.user.user_id && props.listMember.findIndex(y => y.user_join == x.user_id) < 0);
			});
			departments.value = data[0];
		})
		.catch((error) => {
			//toast.error("Tải dữ liệu không thành công!");
			//console.log("Error chat_departments_list_tree");
			if (error && error.status === 401) {
				swal.fire({					
					text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",					
					confirmButtonText: "OK",
				});
				store.commit("gologout");
			}
		});
};

const Del_Message = (msg, i) => {
	swal.fire({
		title: "Thông báo",
		text: "Bạn có muốn xoá tin nhắn này không!",
		icon: "warning",
		showCancelButton: true,
		confirmButtonColor: "#3085d6",
		cancelButtonColor: "#d33",
		confirmButtonText: "Có",
		cancelButtonText: "Không",
	}).then((result) => {
		if (result.isConfirmed) {
			let data = { id: msg.chat_message_id };
			axios
				.post(
					baseUrlCheck + "/api/chat/Delete_Chat",
					data,
					config
				)	
				.then((response) => {
					if (response.data.err == "0") {
						// emitter.emit("emitData", {
						// 	type: "loadListChatGroup",
						// 	data:  null
						// });
						props.funcCallUpdate();
						goBottomChat();
						//Set Realtime
						let title = "vửa xóa một tin nhắn!";
						let listSendTo = [];
						if (props.detailChat.is_group_chat) {
							let listReceiver = props.listMember.map((x) => x.user_join);
							listSendTo = listSendTo.concat(listReceiver);
						}
						else {
							let userReceive = props.detailChat.user_chat != store.getters.user.user_id ? props.detailChat.user_chat : props.detailChat.created_by;
							listSendTo.push(userReceive);
						}
						let mes = {
							//app
							user_id: store.getters.user.user_id,
							sender: store.getters.user.user_id,
							chat_group_id: ms.chat_group_id,
							chat_message_id: ms.chat_message_id,
							full_name: store.getters.user.full_name,
							avatar: store.getters.user.avatar,
							event: "getDelMessage",
							socketid: socket.id,
							Title: title,
							to_user_id: listSendTo[0],
							uids: listSendTo,
							title_chat: props.detailChat.chat_group_name,
						}
						//socket.emit('sendData', mes);
						props.funcCallSocket(mes);
					}
					else {
						console.log(response.data.ms);
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
		}
	});
};

const Remove_Message = (gr) => {
	swal.fire({
		title: "Thông báo",
		text: "Bạn có muốn xoá lịch sử trò chuyện không!",
		icon: "warning",
		showCancelButton: true,
		confirmButtonColor: "#3085d6",
		cancelButtonColor: "#d33",
		confirmButtonText: "Có",
		cancelButtonText: "Không",
	}).then((result) => {
		if (result.isConfirmed) {			
			let data = { id: gr.chat_group_id };
			axios
				.post(
					baseUrlCheck + "/api/chat/Remove_Message",
					data,
					config
				)
				.then((response) => {
					if (response.data.err == "0") {
						toast.success("Xóa dữ liệu thành công!");
						// emitter.emit("emitData", {
						// 	type: "loadListChatGroup",
						// 	data:  null
						// });
						props.funcCallUpdate();
					}
					else {
						console.log(response.data.ms);
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
		}
	});
};
const Del_GroupChat = (gr) => {
	swal.fire({
		title: "Thông báo",
		text: "Bạn có muốn xoá cuộc trò chuyện này không!",
		icon: "warning",
		showCancelButton: true,
		confirmButtonColor: "#3085d6",
		cancelButtonColor: "#d33",
		confirmButtonText: "Có",
		cancelButtonText: "Không",
	}).then((result) => {
		if (result.isConfirmed) {
			let data = { id: gr.chat_group_id };
			axios
				.post(
					baseUrlCheck + "/api/chat/Del_GroupChat",
					data,
					config
				)
				.then((response) => {
					if (response.data.err == "0") {
						toast.success("Xóa dữ liệu thành công!");						
						//localStorage.removeItem("chatGroupID");						
						if (cookies.get("chatGroupID") != null) {
							cookies.remove("chatGroupID");
						}
						if (cookies.get("viewTabChatID") != null) {
							cookies.remove("viewTabChatID");
						}
						// emitter.emit("emitData", {
						// 	type: "loadListChatGroup",
						// 	data:  null
						// });
						props.funcCallUpdate();
						//Set Realtime
						let title = gr.is_group_chat ? (store.getters.user.full_name + " vừa xóa nhóm chat: '" + gr.chat_group_name + "'!") : (store.getters.user.full_name + " vừa xóa cuộc hội thoại với bạn!");
						
						let listSendTo = [];
						if (props.detailChat.is_group_chat) {
							let listReceiver = props.listMember.map((x) => x.user_join);
							listSendTo = listSendTo.concat(listReceiver);
						}
						else {
							let userReceive = props.detailChat.user_chat != store.getters.user.user_id ? props.detailChat.user_chat : props.detailChat.created_by;
							listSendTo.push(userReceive);
						}
						let mes = {
							//app
							user_id: store.getters.user.user_id,
							sender: store.getters.user.user_id,
							chat_group_id: gr.chat_group_id,
							members: props.listMember.map(x => ({ user_join: x.user_join })),
							event: "getDelChat",
							socketid: socket.id,
							//web
							Title: title,
							to_user_id: listSendTo[0],
							uids: listSendTo,
							title_chat: props.detailChat.chat_group_name,
						}
						//socket.emit('sendData', mes);
						props.funcCallSocket(mes);
					}
					else {
						console.log(response.data.ms);
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
		}
	});
};
const Out_GroupChat = (gr, type, user_leave_id, user_remove_id) => {
	swal.fire({
		title: "Thông báo",
		text: type === 1 ? "Bạn có muốn rời khỏi nhóm này không?" : type === 2 ? "Bạn có muốn xóa cuộc hội thoại này không?" : type === 3 ? "Bạn có chắc muốn xóa thành viên ra khỏi nhóm không?" : '',
		icon: "warning",
		showCancelButton: true,
		confirmButtonColor: "#3085d6",
		cancelButtonColor: "#d33",
		confirmButtonText: "Có",
		cancelButtonText: "Không",
	}).then((result) => {
		if (result.isConfirmed) {  
			let data = { user_leave_id: user_leave_id || store.getters.user.user_id, user_remove_id: user_remove_id || "", ms_chat_group_id: gr.chat_group_id };
			axios
				.post(
					baseUrlCheck + "/api/chat/Out_GroupChat",
					data,
					config
				)
				.then((response) => {
					//var groupID_Out = localStorage.setItem("chatGroupID", props.detailChat.chat_group_id);
					cookies.set("chatGroupID", props.detailChat.chat_group_id);
					var groupID_Out = props.detailChat.chat_group_id;
					if (type == 1 || type == 2) {
						//localStorage.removeItem("chatGroupID");						
						if (cookies.get("chatGroupID") != null) {
							cookies.remove("chatGroupID");
						}
						if (cookies.get("viewTabChatID") != null) {
							cookies.remove("viewTabChatID");
						}
						// emitter.emit("emitData", {
						// 	type: "loadListChatGroup",
						// 	data:  null
						// });
						props.funcCallUpdate();
					}
					else {
						var idx = props.listMember.findIndex(x => x.user_join === data.user_leave_id);
						if (idx != -1) {
							props.listMember.splice(idx, 1);
						}						
					}
					
					//Set Realtime
					let title = store.getters.user.full_name + " vừa rời khỏi nhóm chat: '" + gr.chat_group_name + "'!";
					let chatGr = {
						"chat_group_id": gr.chat_group_id,
						"user_chat": gr.user_chat,
						"chat_group_name": gr.chat_group_name,
						"type_group": 0,
						"created_date": moment(ms.ngayGui).format('YYYY-MM-DDTHH:mm:ss'),
						"created_by": store.getters.user.user_id,
						"status": 1,
						"modified_date": moment(ms.ngayGui).format('YYYY-MM-DDTHH:mm:ss'),
						"is_group_chat": true,
					};
					
					let listSendTo = [];
					if (props.detailChat.is_group_chat) {
						let listReceiver = props.listMember.map((x) => x.user_join);
						listSendTo = listSendTo.concat(listReceiver);
					}
					else {
						let userReceive = props.detailChat.user_chat != store.getters.user.user_id ? props.detailChat.user_chat : props.detailChat.created_by;
						listSendTo.push(userReceive);
					}
					let mes = {
						//app
						user_id: store.getters.user.user_id,
						sender: store.getters.user.user_id,
						chat: chatGr,
						members: props.listMember.map(x => ({ user_join: x.user_join })),
						event: "OutUserChat",
						socketid: socket.id,
						chat_group_id: groupID_Out,
						Title: title,
						to_user_id: listSendTo[0],
						uids: listSendTo,
						title_chat: props.detailChat.chat_group_name,
					}
					//socket.emit('sendData', mes);
					props.funcCallSocket(mes);
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
		}
	});
};

const Active_Notify = (chatDetail) => {
	let data = { ...chatDetail };
	axios
		.post(
			baseUrlCheck + "/api/chat/Active_Notify",
			data,
			config
		)
		.then((response) => {
			if (response.data.err == "0") {
				toast.success("Cập nhật cuộc trò chuyện thành công!");
				// emitter.emit("emitData", {
				// 	type: "loadListChatGroup",
				// 	data:  null
				// });
				props.funcCallUpdate();
			}
			else {
				console.log(response.data.ms);
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
const showModalInfoUser = ref(false);
const infoUserChat = ref();
const goInfo = (infochat) => {
	let id_userget = null;
	if (infochat.user_chat != store.getters.user.user_id){
		id_userget = infochat.user_chat;
	}
	else {
		id_userget = infochat.created_by;
	}
	axios
		.post(
			baseUrlCheck + "api/chat/GetDataProc",
			{ 
				str: encr(JSON.stringify({
						proc: "chat_get_info_user",
						par: [
							{ par: "user_id", va: store.getters.user.user_id },
							{ par: "user_getinfo_id", va: id_userget },
						],
					}), SecretKey, cryoptojs
				).toString()
			},
			config
		)
		.then((response) => {
			if (response.data.err != '1') {
				var data = JSON.parse(response.data.data)[0];
				infoUserChat.value = data[0];
				showModalInfoUser.value = true;
			}
		})
		.catch((error) => {
			console.log(error);
			//toast.error("Tải dữ liệu theo dõi không thành công!");
			if (error && error.status === 401) {
				swal.fire({
					text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
					confirmButtonText: "OK",
				});
				store.commit("gologout");
			}
		});
};
const closeDialog = () => {
	displayChat.value = false;
	chat.value = {
		chat_group_id: null,
		chat_group_name: '',
		status: 1,
		organization_id: store.getters.user.organization_id,
		searchU: null,
	};
};
const closeDialogInfo = () => {
	showModalInfoUser.value = false;
}
const funcMesChat = ref();
const mesFuncClick = ref();
const toogleFuncMes = (event, mes, idx) => {
	funcMesChat.value.toggle(event);
	mesFuncClick.value = mes;
	mesFuncClick.value.indexList = idx;
};
const displayModalIframeDoc = ref(false);
const fileShow = ref({
	file_name: "",
	file_path: "",
});
const typeShow = ref(2);
const showfile = (file, typeChat) => {
	if (typeChat == null || (typeChat != null && typeChat.type_message != 3 && typeChat.type_message != 4)) {
		fileShow.value.file_name = file.file_name;
		typeShow.value = 2;	
	}
	else if (typeChat != null && typeChat.type_message == 3) {
		typeShow.value = 3;
	}
	else if (typeChat != null && typeChat.type_message == 4) {
		typeShow.value = 4;
	}
	fileShow.value.file_path = file.file_path;
	fileShow.value.file_type = file.file_type;
	displayModalIframeDoc.value = true;
}
const funcMesFiles = ref();
const fileFuncClick = ref();
const toogleFuncFiles = (event, file) => {
	funcMesFiles.value.toggle(event);
	fileFuncClick.value = file;
};
const openFile = (file, url) => {
	if (url != null) {
		//window.open(baseURL + url, '_blank');
		var url = baseURL + file.file_path;
		var name = file.file_name || "file_download";
		const a = document.createElement("a");
		a.href = basedomainURL + '/Viewer/DownloadFile?url='+ encodeURIComponent(file.file_path) + '&title=' + encodeURIComponent(name);
		a.download = name;
		//a.target = "_blank";
		a.click();
		a.remove();
	}
	else {
		if (file.files.length > 0) {
			//window.open(baseURL + file.files[0].file_path, '_blank');
			var url = baseURL + file.files[0].file_path;
			var name = file.files[0].file_name || "file_download";
			const a = document.createElement("a");
			a.href = basedomainURL + '/Viewer/DownloadFile?url='+ encodeURIComponent(file.files[0].file_path) + '&title=' + encodeURIComponent(name);
			a.download = name;
			//a.target = "_blank";
			a.click();
			a.remove();
		}
	}
}
// emote in chat
const emoteList = ref([]);
const loadEmote = () => {
  axios
    .post(
	  	baseUrlCheck + "api/chat/GetDataProc",
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
const dataEmote = ref({ chat_message_id: null, emote_id: null });
const showEmote = (event, data) => {
	panelEmote.value.toggle(event);
	dataEmote.value.chat_message_id = data.chat_message_id;
};
const checkHover = ref();
const onCheckHover = (item) => {
  checkHover.value = item.emote_id;
};
const hideCheckHover = () => {
  checkHover.value = null;
};
const panelEmoij4 = ref();
const showEmoji = (event, check) => {
	if (check == 1) {
		checkEditEmoij.value = 1;
	} else if (check == 2) {
		checkEditEmoij.value = 2;
	}
  	panelEmoij4.value.toggle(event);
};
const comment_zone_main = ref();
const comment_zone_edit = ref();
const checkEditEmoij = ref(1);
const handleEmojiClick = (event) => {
	if (checkEditEmoij.value == 1)
		if (noiDungChat.value.noiDung) {
			noiDungChat.value.noiDung = noiDungChat.value.noiDung + event.unicode;
		}
		else {
			noiDungChat.value.noiDung = event.unicode;
		}
	else if (checkEditEmoij.value == 2) {
		if (commentEdit.value.des)
			commentEdit.value.des = commentEdit.value.des + event.unicode;
		else commentEdit.value.des = event.unicode;
		let cmtFormat = commentEdit.value.des.replaceAll("<br>", "").replaceAll("</p><p>", "</p><br><p>");
		let cmtData = cmtFormat.replaceAll("<p>", "").replaceAll("</p>", "");
		let arrCmt = cmtData.split("<br>");
		let contentCmt = "";
		arrCmt.forEach((x) => {
			contentCmt += ("<p>" + x + "</p>");
		});
		commentEdit.value.des = contentCmt;
		comment_zone_edit.value[0].setHTML(commentEdit.value.des);
	}
};
// emote with message
const addEmote = (stick) => {
	let data = { chat_group_id: props.detailChat.chat_group_id, chat_message_id: dataEmote.value.chat_message_id, stick_id: stick.emote_id  };
	axios
		.post(
			baseUrlCheck + "/api/chat/Update_Stisk",
			data,
			config
		)
	.then((response) => {
		if (response.data.err !== "1") {
			panelEmote.value.toggle();
			props.refreshData();
			//Set Realtime
			let title = "vửa bày tỏ cảm xúc cho một tin nhắn!";
			
			let listSendTo = [];
			if (props.detailChat.is_group_chat) {
				let listReceiver = props.listMember.map((x) => x.user_join);
				listSendTo = listSendTo.concat(listReceiver);
			}
			else {
				let userReceive = props.detailChat.user_chat != store.getters.user.user_id ? props.detailChat.user_chat : props.detailChat.created_by;
				listSendTo.push(userReceive);
			}
			let mes = {
				//app
				user_id: store.getters.user.user_id,
				sender: store.getters.user.user_id,
				chat_group_id: props.detailChat.chat_group_id,
				chat_message_id: response.data.MessageID,
				stick_id: stick.emote_id,
				created_date: moment(new Date()).format('YYYY-MM-DDTHH:mm:ss'),
				event: "getStick",
				socketid: socket.id,
				//web
				stick_file: rresponsees.data.stick_file || "",
				Title: title,
				to_user_id: listSendTo[0],
				uids: listSendTo,
				title_chat: props.detailChat.chat_group_name,
			}
			//socket.emit('sendData', mes);
			props.funcCallSocket(mes);
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
	  	baseUrlCheck + "api/chat/GetDataProc",
		{ 
			str: encr(JSON.stringify({
					proc: "chat_stisk_by_message",
					par: [
						{ par: "user_id", va: store.getters.user.user_id },
						{ par: "chat_message_id", va: msg.chat_message_id },
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
const changeTabStick = (event) => {
	tabStickActive.value = event.index;
}
// edit message
const isEdit = ref(false);
const EditMessage = (co) => {
	props.listMessage.filter(x => x.IsEdit).forEach(function (r) {
		r.IsEdit = false;
	});
	co.IsEdit = true;
	isEdit.value = true;
	FileAttach.value = [];
	co.Noidung_Edit = co.content_message;
	noiDungChat.value = JSON.parse(JSON.stringify(co));
	noiDungChat.value.noiDung = noiDungChat.value.content_message;
	//noiDungChat.value.noiDung = co.content_message;
	funcMesChat.value.toggle();
	if (co.chat_parent_id != null) {
		props.listMessage.filter(x => x.IsReply).forEach((r, idx) => {
			r.IsReply = false;
		});
		IsReply.value = true;
		ReplyID.value = co.chat_parent_id;
		var replyMes = props.listMessage.filter(x => x.chat_message_id == co.chat_parent_id).length > 0 
							? props.listMessage.filter(x => x.chat_message_id == co.chat_parent_id)[0]
							: null;
		if (replyMes != null) {
			replyMes.IsReply = true;
		}
		tinnhanreply.value = replyMes;
	}
	goBottomChat();
};

const cancelEditMessage = (u) => {
	u.IsEdit = !(u.IsEdit || false);
	isEdit.value = false;
	noiDungChat.value = { noiDung: "" };

	if (u.chat_parent_id != null) {
		var co = props.listMessage.find(x => x.chat_message_id === ReplyID.value);
		IsReply.value = false;
		ReplyID.value = null;
		tinnhanreply.value = null;
		co.IsReply = false;
		FileAttach.value = [];
	}
};

const clickAddUser = (m, us, u, i) => {
	if (typeGroupChat.value == 1) {
		if (us && us.length > 0) {
			let fil = us.filter(x => x.user_id === u.user_id || x.user_join === u.user_id).length > 0;
			if (!fil) {
				u.user_join = u.user_id;
				us.push(u);
			}
		}
		else {
			u.user_join = u.user_id;
			us.push(u);
		}
		m["searchU" + i] = "";
		// var iC = filterUserCopy.value.findIndex(x => x.user_id === u.user_id);
		// if (iC !== -1) {
		// 	filterUserCopy.value.splice(iC, 1);
		// }
	}
};

const submitted = ref(false);
const savingChat = ref(false);
const saveGroupChat = (isFormValid) => {
	if (savingChat.value == true) {
		return;
	}
	submitted.value = true;
	if (typeGroupChat.value == 0) {
		
	}
	else {
		if (!isFormValid) {
			return;
		}
		chat.value.is_group_chat = true;
	}
	var ms = { chat_message_id: null }; 
	let formData = new FormData();
	for (var i = 0; i < files.length; i++) {
		let file = files[i];
		formData.append("avatarGroup", file);
	}
	let listMemberChat = [];
	if (chat.value.is_group_chat == true){
		listMemberChat = props.listMember;
	}
	formData.append("models", JSON.stringify(chat.value));
	formData.append("members", JSON.stringify(listMemberChat));
	formData.append("messages", JSON.stringify(ms));
	savingChat.value = true;
	swal.fire({
		width: 110,
		didOpen: () => {
		swal.showLoading();
		},
	});
	axios({
		method: "post",
		url: baseUrlCheck + `/api/chat/Add_Chat`,
		data: formData,
		headers: {
			Authorization: `Bearer ${store.getters.token}`,
		},
	})
    .then((response) => {
		savingChat.value = false;
		if (response.data.err != "1") {
			swal.close();
			//toast.success("Cập nhật cuộc trò chuyện thành công!");
			displayChat.value = false;
			loadDataGroupChat();
			// emitter.emit("emitData", {
			// 	type: "loadListChatGroup",
			// 	data:  null
			// });
			props.funcCallUpdate();
		} else {
			swal.fire({
				title: "Thông báo!",
				text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
				icon: "error",
				confirmButtonText: "OK",
			});
		}
    })
    .catch((error) => {
		swal.close();
		savingChat.value = false;
		swal.fire({
			title: "Thông báo!",
			text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
			icon: "error",
			confirmButtonText: "OK",
		});
    });
};

// share chat
const headerShareChat = ref();
const displayShareChat = ref(false);
const msgShare = ref();
const ShareMsg = (mes, overlay) => {
	if (overlay != null) {
		funcMesChat.value.toggle();
	}
	listChatShare.value = [];
	dataListChat.value.forEach((el, idx) => {
		el.checkShare = false;
	});
	filterUser.value.forEach((el, idx) => {
	 	el.checkShare = false;
	});
	headerShareChat.value = "Chia sẻ tin nhắn";
	msgShare.value = { ...mes };	
	switch (mes.type_message) {
		case 1:
			msgShare.value.FileAttach_Share = [ ...msgShare.value.files ];
			if (msgShare.value.FileAttach_Share && msgShare.value.FileAttach_Share.length > 0) {
				msgShare.value.FileAttach_Share.forEach(function (r) {
					r.loai = 1;
				});
			}
			break;
		case 2:
			msgShare.value.FileAttach_Share = [ ...msgShare.value.files ];
			if (msgShare.value.FileAttach_Share && msgShare.value.FileAttach_Share.length > 0) {
				msgShare.value.FileAttach_Share.forEach(function (r) {
					r.loai = 2;
				});
			}
			break;
		default:
			msgShare.FileAttach_Share = [];
			break;
		
	}

	displayShareChat.value = true;
};
const dataListChat = ref();
const loadDataGroupChat = () => {
	axios
      .post(
		baseUrlCheck + "api/chat/GetDataProc",
		{ 
			str: encr(JSON.stringify({
					proc: "chat_group_list",
					par: [
						{ par: "organization_id", va: store.getters.user.organization_id },
						{ par: "user_id ", va: store.getters.user.user_id },
						{ par: "search", va: "" },
						{ par: "type_chat", va: -1 },
						{ par: "sort", va: 'modified_date' },
						//{ par: "chat_id_active", va: localStorage.getItem("chatGroupID") },
						{ par: "chat_id_active", va: cookies.get("chatGroupID") },
					],
				}), SecretKey, cryoptojs
			).toString()
		},
		config
      )
      .then((response) => {
		let data = JSON.parse(response.data.data);
		if (data.length > 0 && data[0].length > 0) {
			data[0].forEach((el, idx) => {
				el.is_order = idx;
			});
			dataListChat.value = data[0].filter(grChat => grChat.displayInList == 1);
		}
		else {
			dataListChat.value = [];
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
loadDataGroupChat();
const share = ref({
	searchU: "",
});
const filterListChatRecent = () => {
	// dataListChat.value.forEach((el, idx) => {
	// 	el.checkShare = false;
	// });
	if (share.value.searchU != null && share.value.searchU.trim() != "") {
		let keySearchShare = change_unsigned(share.value.searchU);
		return dataListChat.value.filter(x => x.chat_group_id !== props.detailChat.chat_group_id && change_unsigned(x.chat_group_name || "").indexOf(keySearchShare) >= 0);
	}
	else {
		return dataListChat.value.filter(x => x.chat_group_id !== props.detailChat.chat_group_id);
	}
};
const filterListUserAll = () => {
	// filterUser.value.forEach((el, idx) => {
	// 	el.checkShare = false;
	// });
	if (share.value.searchU != null && share.value.searchU.trim() != "") {
		let keySearchShare = change_unsigned(share.value.searchU);
		return filterUser.value.filter(x => x.user_id !== store.getters.user.user_id && (change_unsigned(x.full_name || "").indexOf(keySearchShare) >= 0 || x.user_id.indexOf(keySearchShare) >= 0));
	}
	else {
		return filterUser.value.filter(x => x.user_id !== store.getters.user.user_id);
	}
};
const listChatShare = ref([]);
const clickAddShareMsg = (u, type) => {
	if (u.checkShare) {
		u.checkShare = true;
		let uShare = { chat_group_id: u.chat_group_id, user_id: u.user_id, avatar: type == 1 ? u.avatar_group : u.avatar, full_name: type == 1 ? u.chat_group_name : u.full_name, last_name: type == 1 ? u.chat_group_lastname : u.last_name };
		if (listChatShare.value.filter(x => (x.chat_group_id == uShare.chat_group_id && uShare.chat_group_id != null) || (x.user_id == uShare.user_id && uShare.user_id != null)).length == 0) {
			listChatShare.value.push(uShare);
		}
	}
	else {
		u.checkShare = false;
		let uShare = { chat_group_id: u.chat_group_id, user_id: u.user_id, avatar: type == 1 ? u.avatar_group : u.avatar, full_name: type == 1 ? u.chat_group_name : u.full_name, last_name: type == 1 ? u.chat_group_lastname : u.last_name };
		if (listChatShare.value.filter(x => (x.chat_group_id == uShare.chat_group_id && uShare.chat_group_id != null) || (x.user_id == uShare.user_id && uShare.user_id != null)).length > 0) {
			let idxDel = listChatShare.value.findIndex(x => (x.chat_group_id == uShare.chat_group_id && uShare.chat_group_id != null) || (x.user_id == uShare.user_id && uShare.user_id != null));
			listChatShare.value.splice(idxDel, 1);
		}
	}
};
const shareMesChat = () => {
	if (loadding.value) {
		return false;
	}
	if (listChatShare.value.length === 0) {
		swal.fire({
			type: 'error',
			icon: 'error',
			title: '',
			text: 'Vui lòng chọn nhóm, người được chia sẻ !'
		});
		return false;
	}
	var msg = { ...msgShare.value };
	//msg.chat_parent_id = null;
	if (!msg.content_message && (msg.FileAttach_Share == null || msg.FileAttach_Share.length === 0)) {
		swal.fire({
			type: 'error',
			icon: 'error',
			title: '',
			text: 'Vui lòng nhập nội dung chat !'
		});
		return false;
	}
	loadding.value = true;
	//swal.showLoading();

	//Progress File
	var arrChatID = listChatShare.value.filter(x => x.chat_group_id != null).map(x => x.chat_group_id);
	var arrUser_ID = listChatShare.value.filter(x => x.chat_group_id == null).map(x => x.user_id);
	
	var formData = new FormData();
	formData.append("MessageID", msgShare.value.chat_message_id);
	formData.append("arrChatID", JSON.stringify(arrChatID));
	formData.append("arrUser_ID", JSON.stringify(arrUser_ID));
	formData.append("models", JSON.stringify(msg));
	axios({
		method: 'post',
		url: baseUrlCheck +
			`/api/chat/Share_ChatMessage`,
		data: formData,
		headers: {
			Authorization: `Bearer ${store.getters.token}`,
		},
	})
	.then((response) => {
		loadding.value = false;
		swal.close();
		
		if (response.data.err !== "0") {
			swal.fire({
				icon: 'error',
				type: 'error',
				title: '',
				text: response.data.ms ? response.data.ms : 'Có lỗi khi chia sẻ tin nhắn !'
			});
			return false;
		}

		var UpFile = false;
		if (msg.FileAttach_Share != null && msg.FileAttach_Share.length > 0) {
		 	UpFile = true;
		}
		// emitter.emit("emitData", {
		// 	type: "loadListChatGroup",
		// 	data:  null
		// });
		props.funcCallUpdate();
		displayShareChat.value = false;
		
		//Set Realtime		
		if (response.data.chats && response.data.chats.length > 0) {
			response.data.chats.forEach((ch, indexChat) => {
				response.data.mess.filter(x => x.chat_group_id === ch.chat_group_id).forEach((message, idxMsg) => {
					let title = "";
					let listSendTo = [];
					if (props.detailChat.is_group_chat) {
						title = ("vừa gửi tin nhắn đên nhóm chat '" + props.detailChat.chat_group_name + "'!");
						let listReceiver = props.listMember.map((x) => x.user_join);
						listSendTo = listSendTo.concat(listReceiver);
					}
					else {
						title = "vửa gửi cho bạn một tin nhắn!";
						let userReceive = props.detailChat.user_chat != store.getters.user.user_id ? props.detailChat.user_chat : props.detailChat.created_by;
						listSendTo.push(userReceive);
					}
					let mes = {
						//app
						uuid: message.chat_message_id,
						chat_group_id: ch.chat_group_id,
						user_id: store.getters.user.user_id,
						sender: store.getters.user.user_id,
						content_message: ms.content_message,
						type_message: message.type_message,
						date_send: moment(ms.created_date).format('YYYY-MM-DDTHH:mm:ss'),
						full_name: store.getters.user.full_name,
						avatar: store.getters.user.avatar,
						status: 0,
						isAdd: true,
						event: "getSendMessage",
						socketid: socket.id,
						chat_message_id: message.chat_message_id,
						chat_parent_id: message.chat_parent_id, // parent id message
						// file_name: message.file_name,
						// file_type: message.file_type,
						// file_path: message.file_path,
						last_name: store.getters.user.last_name ? store.getters.user.last_name 
									: (store.getters.user.full_name != null ? store.getters.user.full_name.substring(store.getters.user.full_name.trim().lastIndexOf(' ') + 1) : 'anonymous'),
						Title: title,
						UpFile: UpFile,
						to_user_id: listSendTo[0],
						uids: listSendTo,
						title_chat: props.detailChat.chat_group_name,
					}
					//socket.emit('sendData', mes);
					props.funcCallSocket(mes);
				});
			});
		}
		
	});
};
const listActiveTabInfoChat = ref([4]);
const showTabInfoChat = () => {
	props.detailChat.IsInfoChat = !(props.detailChat.IsInfoChat || false);
	//localStorage.setItem("viewTabChatID", props.detailChat.IsInfoChat);
	cookies.set("viewTabChatID", props.detailChat.IsInfoChat);
};
const SeenMess = (MessageID, User_ID) => {
	if (props.listMessage && props.listMessage.length > 0) {
		let newmess = props.listMessage.find(x => x.chat_message_id == MessageID);
		if (newmess && newmess.seens.filter(x => x.user_id == User_ID).length == 0) {
			let idxcm = props.listMessage.findIndex(x => x.chat_message_id == MessageID);
			props.listMessage.forEach(function (ms, k) {
				if (k < idxcm && ms.seens && ms.seens.length > 0) {
					let idxmb = ms.seens.findIndex(x => x.user_id == User_ID);
					ms.seens.splice(idxmb, 1);
				}
			});
			let ns = filterUser.value.find(x => x.user_id == User_ID);
			newmess.seens.unshift({ user_id: ns.user_id, full_name: ns.full_name, last_name: ns.last_name, avatar: ns.avatar, viewdate: new Date() });
			UpdateSeenMess(newmess, User_ID);
		}
	}
};
const UpdateSeenMess = (newmess, User_ID) => {
	axios
		.post(
			baseUrlCheck + "api/chat/GetDataProc",
			{ 
				str: encr(JSON.stringify({
						proc: "chat_update_seen_msg",
						par: [
							{ par: "chat_group_id", va: newmess.chat_group_id },
							{ par: "chat_message_id", va: newmess.chat_message_id },
							{ par: "user_id", va: User_ID },
						],
					}), SecretKey, cryoptojs
				).toString()
			},
			config
    ).then((response) => {
		//var data = response.data.data;
		goBottomChat();
	})
    .catch((error) => {
      swal.fire({
        title: "Error!",
        text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
        icon: "error",
        confirmButtonText: "OK",
      });
    });
};
const listUserSeen = ref([]);
const displaySeen = ref(false);
const showModalUserSeen = (msg) => {
	if (props.detailChat.is_group_chat) {
		displaySeen.value = true;
		axios
			.post(
				baseUrlCheck + "api/chat/GetDataProc",
				{
					str: encr(JSON.stringify({
						proc: "chat_list_user_seen",
						par: [
							{ par: "chat_group_id", va: msg.chat_group_id },
							{ par: "chat_message_id", va: msg.chat_message_id },
							{ par: "user_id", va: store.getters.user.user_id },
						],
					}), SecretKey, cryoptojs
					).toString()
				},
				config
			).then((response) => {
				if (response.data.err != "1") {
					var data = JSON.parse(response.data.data);
					listUserSeen.value = data[0];
				}
				else {
					listUserSeen.value = [];
				}
			})
			.catch((error) => {

			});
	}
};
const openSearchMsg = ref(false);
const openResultSearchMsg = ref(false);
const SearchMsg = ref("");
const Open_SearchMessage = (group) => {
	SearchMsg.value = "";
	openResultSearchMsg.value = false;
	msgActiveSearch.value = null;
	listMsg_Search.value = [];
	if (!openSearchMsg.value) {
		openSearchMsg.value = true;
	}
	else {
		openSearchMsg.value = false;
	}
};
const listMsg_Search = ref([]);
const Search_Message = () => {
	if (SearchMsg.value != null && SearchMsg.value.trim() != "") {
		let keySearchMsg = change_unsigned(SearchMsg.value);
		listMsg_Search.value = props.listMessage.filter(x => x.content_message != null && change_unsigned(x.content_message).includes(keySearchMsg));
		listMsg_Search.value = listMsg_Search.value.reverse();
		openResultSearchMsg.value = true;
	}
	else {
		listMsg_Search.value = [];
	}
};
const resetSearchMessage = () => {
	SearchMsg.value = "";
	openSearchMsg.value = false;
	openResultSearchMsg.value = false;
	msgActiveSearch.value = null;
	listMsg_Search.value = [];
};
const msgActiveSearch = ref();
const goToPositionMsg = (msg) => {
	let indexMsg = props.listMessage.findIndex(x => x.chat_message_id == msg.chat_message_id);
	msgActiveSearch.value = msg.chat_message_id;
	if (indexMsg >= 0 && indexMsg < props.listMessage.length - 1) {
		setTimeout(() => {
			let elmnt = document.getElementById("chat_"+(indexMsg == 0 ? 0 : (indexMsg-1)));
			if (elmnt != null) {
				elmnt.scrollIntoView();
			}
		}, 200);
	}
	else {
		goBottomChat();
	}
}
const scrollBoxChat = () => {
	let bottomChatNow = document.getElementById("task-comment").scrollTop;
    if (bottomChat.value - bottomChatNow > heightFinalChat.value) {
        document.getElementById('id-btn-goBottom').style.visibility = "visible";
    } else {
 		document.getElementById('id-btn-goBottom').style.visibility = "hidden";
    }
};
// const initSocketDataMessage = (data) => {
// 	switch (data["event"]) {
// 		case "getSendMessage":
// 			SeenMess(data["chat_message_id"], data["sender"]);			
// 			// emitter.emit("emitData", {
// 			// 	type: "loadListChatGroup",
// 			// 	data: null
// 			// });
// 			props.funcCallUpdate();
// 			// if (data["UpFile"]) {
// 			// 	listFileTailieu();
// 			// }
// 			break;
// 		case "getDelMessage":
// 			if (data["chat_group_id"] == props.detailChat.chat_group_id) {
// 				// emitter.emit("emitData", {
// 				// 	type: "loadListChatGroup",
// 				// 	data: null
// 				// });
// 				props.funcCallUpdate();
// 			}
// 			break;
// 		case "OutUserChat":
// 			if (data["chat_group_id"] == props.detailChat.chat_group_id) {
// 				// emitter.emit("emitData", {
// 				// 	type: "loadListChatGroup",
// 				// 	data: null
// 				// });
// 				props.funcCallUpdate();
// 			}
// 			break;
// 	}
// };
// const initSocketDataChat = (data) => {
// 	switch (data["event"]) {
// 		case "getDelChat":
// 			if (data["chat_group_id"] == props.detailChat.chat_group_id) {
// 				// emitter.emit("emitData", {
// 				// 	type: "loadListChatGroup",
// 				// 	data: null
// 				// });
// 				props.funcCallUpdate();
// 			}
// 			break;
// 	}
// };

onMounted(() => {
	listFileTailieu();
	loadEmote();
	setdropZone("drop-zone");
	goBottomChat('first');
	listDepartmentsUser();
	return {
		listFileTailieu,
		loadEmote,
	}
});
</script>
<template>
	<div class="main-chat">
		<div class="main-center m-0 upload-drop-zone-1 flex-1" id="drop-zone" mid="ModalAddFile" 
			:class="{ 'mr-0': !props.detailChat.IsInfoChat }" 
			style="position: relative !important;"
		>
			<div class="box-header format_center p-2" style="justify-content: space-between; height:62px; border-bottom: 0.4rem solid #edf0f2;">
				<div class="flex format_text" style="flex: 1; ">
					<div class="group-content-icon">
						<div class="card-users m-0">
							<ul v-if="props.detailChat.is_group_chat" 
								@click="Edit_GroupChat(props.detailChat)" 
								class="format_center group" 
								style="cursor: pointer;">
								<li style="position: relative;">
									<div class="flex" style="justify-content:center;line-height:1.5;position: relative;">
										<img class="ava" width="48" height="48" 
											v-bind:src="props.detailChat.avatar_group
														? basedomainURL + props.detailChat.avatar_group
														: basedomainURL + '/Portals/Image/image_group_user.jpg'
													"
											@error="$event.target.src = basedomainURL + '/Portals/Image/image_group_user.jpg'"
											style="border: 1px solid #ccc;"
										/>
										<span class="online" style="bottom:-1px;right:-1px;"></span>
									</div>
								</li>
								<li style="margin-left: 10px; text-align: left">
									<h4 class="m-0 pb-2" style="font-size: 1.25rem;">{{props.detailChat.chat_group_name}}</h4>
									<span class="tv flex" style="font-size: 1rem;">
										<i class="pi pi-user mr-1" style="font-size:1rem;"></i>
										{{props.listMember.length}} thành viên
									</span>
								</li>
							</ul>
							<ul v-if="!props.detailChat.is_group_chat" class="format_center m-0 p-0">
								<li @click="goInfo(props.detailChat)" class="flex">
									<div class="flex" style="justify-content:center;line-height:1.5;position: relative">											
										<img class="ava" width="48" height="48" 
											v-bind:src="props.detailChat.user_chat_avatar
														? basedomainURL + props.detailChat.user_chat_avatar
														: basedomainURL + '/Portals/Image/noimg.jpg'
													"
											@error="$event.target.src = basedomainURL + '/Portals/Image/nouser1.png'"
											v-if="props.detailChat.user_chat_avatar"
											style="border:1px solid #ccc;"
										/>
										<Avatar v-else
											class="avt-replace"
											size="large"
											shape="circle"
											v-bind:label="(props.detailChat.chat_group_lastname ?? '').substring(0, 1)"
											style="cursor: pointer;width: 48px; height: 48px;border:1px solid #ccc;"
											:style="{ background: bgColor[0] + '!important'}"
										/>
										<span class="online" style="bottom:-1px;right:-1px;" v-if="props.detailChat.Online"></span>
										<span class="offline" style="bottom:-1px;right:-1px;" v-else></span>
									</div>
									<div class="access-info ml-2">
										<h4 class="m-0" style="font-size: 1.25rem;">{{props.detailChat.user_chat_name || ''}}</h4>
										<small style="font-size:1rem;" v-if="props.detailChat.Online">Đang hoạt động</small>
										<small style="font-size:1rem;" v-else>Vắng mặt</small>
										<!-- <small v-if="!u.Online && u.lastOnline" tooltip-placement="bottom" uib-tooltip="{{u.lastOnline|date:'HH:mm dd/MM'}}">Hoạt động {{u.lastOnline|timeago}}</small> -->
									</div>
								</li>
							</ul>
						</div>
					</div>
				</div>
				<div class="flex top-bar" style="width: auto;">
					<ul class="utop flex m-0 format_center justify-content-end">
						<li style="text-align: right; min-width: max-content; margin: 0 !important;">
							<button class="btn btn-new p-2" :class="openSearchMsg ? 'active-info-chat' : ''" 
								@click="Open_SearchMessage(props.detailChat)" v-tooltip.top="'Tìm kiếm tin nhắn'">
								<i style="font-size: 1.5rem; cursor: pointer;" class="pi pi-search cv-search-icon"></i>
							</button>
						</li>
						<li v-if="props.detailChat.is_group_chat" style="text-align: right; padding-left: 5px; min-width: max-content; margin: 0 !important;">
							<button class="btn btn-new p-2" @click="Edit_GroupChat(props.detailChat)" v-tooltip.top="'Thêm thành viên vào nhóm'">
								<i style="font-size: 1.5rem; cursor: pointer;" class="pi pi-user-plus cv-search-icon"></i>
							</button>
						</li>
						<!-- <li style="text-align: right; padding-right: 5px; min-width: max-content; margin: 0 0 0 0.5rem !important;">
							<button class="btn btn-new p-2"
								:class="{ 'IsLockCall': !DataDichvu.IsUse }"
								@click="callVideo(false)" v-tooltip.top="'Cuộc gọi thoại'"
							>
								<i style="font-size: 1.5rem; cursor: pointer;" class="pi pi-phone cv-search-icon"></i>
							</button>
						</li>
						<li style="text-align: right; padding-right: 5px; min-width: max-content; margin: 0 0 0 0.5rem !important;">
							<button class="btn btn-new p-2" 
								:class="{ 'IsLockCall': !DataDichvu.IsUse }"
								@click="callVideo(true)" v-tooltip.top="'Cuộc gọi video'"
							>
								<i style="font-size: 1.5rem; cursor: pointer;" class="pi pi-video cv-search-icon"></i>
							</button>
						</li> -->
						<li style="text-align: right; padding-right: 5px; min-width: max-content; margin: 0 0 0 0.5rem !important;">
							<button class="btn btn-new p-2" :class="props.detailChat.IsInfoChat ? 'active-info-chat' : ''" 
								@click="showTabInfoChat()"
								v-tooltip.top="'Thông tin hội thoại'">
								<i style="font-size: 1.5rem; cursor: pointer;" class="pi pi-map cv-search-icon"></i>
							</button>
						</li>
					</ul>
				</div>
			</div>
			<div class="flex shadow-1" 
				style="border-bottom: 1px solid #dee2e6;background-color: #fff;color: #000;
						padding: 0.5rem 0.9rem !important;margin-top: -0.25rem;
						position: absolute;width: 100%;z-index: 1000;" 
				v-if="openSearchMsg"
			>
				<span class="p-input-icon-left w-full flex search-filter">
					<i class="pi pi-search" />
					<InputText type="text" spellcheck="false" v-model="SearchMsg"						 
						placeholder="Tìm kiếm tin nhắn"
						class="inputtext-filter"
						v-focus
						v-on:keypress.enter.exact.prevent="Search_Message()"
						style="border-radius: 15px;margin-right:1rem;flex:1;"
					/>
					<Button icon="pi pi-times" label="Đóng" class="p-button-rounded p-button-secondary" @click="resetSearchMessage" />
				</span>
			</div>
			<div class="bg-white">
				<div id="taskmessagepanel" class="scroll-outer" 
					style="min-height: calc(100vh - 200px); max-height: calc(100vh - 200px); background-color: #fff; overflow-y: auto; overflow-x: hidden; " 
					buffered-scroll-up="loadMore(props.listMessage)">
					<div class="scroll-inner scroll-inner-chat" v-if="!IsCall">
						<div class="div-content-chat">
							<div class="task-comment w-full" id="task-comment" @scroll="scrollBoxChat">
								<div v-for="(u, index) in props.listMessage" :id="index == props.listMessage.length - 1 ? 'chat_final' : ('chat_' + index)" :key="index" ref="index">
									<div class="message-feed media noidungchat" 
										style="overflow: -webkit-paged-y;display: inline-block;width: 100%;float: left;"
										v-if="index == 0 || props.listMessage[index-1].strCreateDate != u.strCreateDate">
										<span class="borderchat"></span>
										<span class="spandatechat">
											{{ u.strCreateDate }}
										</span>
									</div>
									<div v-if="u.messtitle" class="message-feed media noidungchat" style="overflow: -webkit-paged-y;display: inline-block;width: 100%;float: left">											
										<a :href="f.content_message" target="_blank" v-if="u.content_message.includes('http://%') || u.content_message.includes('https://%')">
											<span v-html="f.content_message"></span>
										</a>
										<span v-else v-html="u.content_message" class="spandatechat mt-1" style="width: max-content; background-color: transparent; color: #aaa; max-width: 300px; white-space: normal; overflow: hidden; text-overflow: ellipsis; "></span>
										<span class="borderchat" style="width:0"></span>
									</div>
									<div v-if="!u.messtitle" class="row-comment" style="padding: 15px 0px 8px 0px; background-color: #fff;" 
										:class="{ 'pd-loopfirst': u.loopmefirst, 'pd-loop': u.loopme, 'pd-looplast': u.loopmelast,'pd-left' : u.IsMe, 'pd-right' : !u.IsMe, 'isGroup': props.detailChat.is_group_chat }">
										<div v-if="!u.IsMe" class="r-ava format_center" style="height: max-content;" :class="{ 'loopme': u.loopme }">
											<img class="ava" 
												v-bind:src="u.user_create_avatar
															? basedomainURL + u.user_create_avatar
															: basedomainURL + '/Portals/Image/noimg.jpg'
														"
												@error="$event.target.src = basedomainURL + '/Portals/Image/nouser1.png'"
												v-if="u.user_create_avatar"
												style="cursor: pointer;border:1px solid #e9e9e9;width:3rem;height:3rem;"
											/>
											<Avatar v-else
												class="avt-replace"
												size="large"
												shape="circle"
												v-bind:label="u.user_create_avatar ? '' : (u.last_name ?? '').substring(0, 1)"
												style="cursor: pointer;width:3rem; height:3rem;border:1px solid #e9e9e9;"
												:style="{ background: (props.detailChat.is_group_chat ? bgColor[index % 7] : bgColor[0]) + '!important'}"
											/>
										</div>
										<div class="r-cbox m-0"
											:class="{ 'classMsgActiveSearch': msgActiveSearch == u.chat_message_id }"
											style="position: relative;max-width: 80%;"
											:style="(!(u.files != null && u.files.length > 0) || u.IsEdit) ? 'padding: 0.5rem 0.5rem 1rem;' + (u.IsMe ? 'background: #DBF1FF;' : '') :  (u.IsMe ? 'background: #DBF1FF;' : '')"
										>
											<div class="r-cname mx-1" style="color: #aaa;">
												<span v-if="(props.detailChat.is_group_chat && !u.IsMe && !(u.files != null && u.files.length > 0))">{{u.full_name}}</span>
											</div>
											<div v-if="u.type_message == 1 && (u.files != null && u.files.length > 0)" class="r-cm p-0 m-0">
												<div class="image_preview_chat" style="max-width: 300px;" v-for="(imageFile, idxImg) in u.files" :key="idxImg">
													<Image v-if="imageFile.is_image == '1'"
														class="flex"
														:src="basedomainURL + (imageFile.file_path ||'/Portals/Image/noimg.jpg')"
														style="width: 100%; height: 100%; object-fit: contain;"
														preview
													/>
													<div class="pt-1 pb-3 123" v-if="imageFile.is_image == '1'">
														<span class="r-cdate fw-400">
															{{ u.created_date ? moment(new Date(u.created_date)).format("HH:mm DD/MM") : '' }}
														</span>
													</div>
												</div>
											</div>
											<div v-if="u.content_message" class="r-cm p-0 mt-0" 
												style="word-break:break-word; margin-bottom: 0px;border-radius: 20px;width: calc(100% - 1.25rem);"
												:style="u.IsMe ? 'background: #DBF1FF;' : ''">
												<div class="reply-chat show-reply" v-if="u.ParentComment" style="padding: 10px;border-bottom: 0.5px solid #ccc;">
													<div>
														<div class="content-reply">
															<font-awesome-icon icon="fa-solid fa-quote-right" style="font-size: 1rem; color: gray;padding-bottom: 5px;" />
															<div style="display: inline-block; padding: 5px 10px;" class="bind-chat-html" v-html="u.ParentComment.content_message"></div>
															<div v-if="u.ParentComment.file_path" class="r-cm p-0 m-0">
																<div style="max-width: 150px;">
																	<a v-bind:href="basedomainURL+u.ParentComment.file_path" data-fancybox :data-caption="u.ParentComment.file_name">
																		<img v-bind:src="basedomainURL + (u.ParentComment.file_path ||'/Portals/Image/noimg.jpg')" on-error="/Portals/Image/noimg.jpg" style="width: 100%; height: 100%; object-fit: contain; border-radius: 10px;" />
																	</a>
																</div>
															</div>
														</div>
														<div class="name-date-reply" style="text-align: left; white-space: nowrap;">
															<span style="color: #888;font-size: 12px;">
																{{u.ParentComment.user_chat_name + ', ' + 
																	(moment(new Date(u.ParentComment.created_date)).format("DD/MM/YYYY") == moment(new Date()).format("DD/MM/YYYY") 
																	? ("Hôm nay lúc " + moment(new Date(u.ParentComment.created_date)).format("HH:mm"))
																	: moment(new Date(u.ParentComment.created_date)).format("DD/MM/YYYY"))
																}}
															</span>
														</div>
													</div>
												</div>
												<a :href="u.content_message" target="_blank" v-if="u.content_message.includes('http://') || u.content_message.includes('https://')">
													<div v-html="u.content_message" style="display: inline-grid; text-align: -webkit-left; padding: 5px 10px; "></div>
												</a>
												<div v-else v-html="u.content_message" style="display: inline-grid; text-align: -webkit-left; padding: 5px 10px; "></div>
												<div>
													<span class="r-cdate fw-400">
														{{ u.created_date ? moment(new Date(u.created_date)).format("HH:mm DD/MM") : '' }}
													</span>
												</div>
											</div>

											<div class="r-file" v-if="u.type_message != 0 && u.type_message != 1 && u.files != null && u.files.length>0" style="padding:0">
												<ul>
													<li v-for="(f, indexFile) in u.files" :key="indexFile" class="border-none px-0 py-2">
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
													<div class="pt-0 pb-3">
														<span class="r-cdate fw-400">
															{{ u.created_date ? moment(new Date(u.created_date)).format("HH:mm DD/MM") : '' }}
														</span>
													</div>
												</ul>
											</div>
											<div class="r-action">
												<div class="position-absolute flex" style="bottom: -13px; min-width: 100%;">
													<div @click="getStick(u)" style="cursor:pointer">
														<ul class="lis-n p-0 mr-2" v-if="u.sticks && u.sticks.length > 0" style="box-shadow: none;">
															<li class="flex" v-for="(st, idxSt) in u.sticks" :key="idxSt" style="border-radius: 50%;">
																<img v-bind:src="basedomainURL+st.stick_file" style="width: 25px;" />
															</li>
															<li class="flex" v-if="u.countstick - 3 > 0" style="border-radius: 50%;">
																<span class="flex pr-1" style="align-items: center;"> +{{u.countstick - 3}}</span>
															</li>
														</ul>
													</div>
													<div class="r-action hoverMes" style="cursor:pointer">
														<ul v-if="!u.IsEdit" class="lis-n p-0" style="background-color: transparent; box-shadow: none;">
															<li>
																<a v-if="!u.IsMe" class="mr-1"
																	@click="showEmote($event, u)">
																	<span class="badge-2 fw-400 m-0" style="font-size: 12px; " v-tooltip.top="'Tương tác'">
																		<i class="pi pi-thumbs-up c-blue-700" style="font-size: 1.25rem;"></i>
																	</span>
																</a>
															</li>
															<li>
																<a @click="u.IsReply?HuyReply():Reply(u)" style="margin-right: 5px;">
																	<span class="badge-2 fw-400 m-0" style="font-size: 12px; " v-tooltip.top="u.IsReply?'Hủy':'Trả lời '">
																		<i v-if="u.IsReply" class="pi pi-times" style="font-size: 1.25rem; color:red;"></i> 
																		<font-awesome-icon v-else icon="fa-solid fa-quote-right" style="font-size: 1.25rem; color: orange;" />
																		<span style="vertical-align: super;" v-if="u.CountReply != null && u.CountReply.length > 0">
																			{{((u.CountReply.length > 0) ? '('+u.CountReply.length+')' : '') }}
																		</span>
																	</span>
																</a>
															</li>
															<li>
																<a @click="ShareMsg(u)">
																	<span class="badge-2 fw-400 m-0" style="font-size: 12px; " v-tooltip.top="'Chia sẻ'">
																		<font-awesome-icon icon="fa-solid fa-share" style="font-size: 1.25rem; color: #1976d2;" /> 
																	</span>
																</a>
															</li>
														</ul>
													</div>
												</div>
											</div>
											<div style="position: absolute;top: 0;right: 0;">
												<div :id="'menu' + u.chat_message_id" class="btn-group" role="group">
													<Button
														type="button"
														class="btn cur-p no-after btn-mes-func"
														icon="pi pi-ellipsis-h"
														@click="toogleFuncMes($event, u, index)"
														aria-haspopup="true"
														aria-controls="overlay_panelFuncMes"
													/>
												</div>
											</div>
										</div>
									</div>
									<div v-if="u.seens && u.seens.length > 0 && (u.IsMe || props.detailChat.is_group_chat)" class="pB-5" style="display: flex;flex-direction: column;padding-right: 8px;align-items: end;">
										<span class="format_center" style="font-size: 12px;">Đã xem</span>
										<div class="card-users m-0">
											<ul class="format_center" style="margin:0.25rem 0 0;" @click="showModalUserSeen(u)">
												<li class="flex" v-for="(sn, idx) in u.seens" :key="idx">
													<img class="ava" 
														v-bind:src="sn.avatar
															? basedomainURL + sn.avatar
															: basedomainURL + '/Portals/Image/noimg.jpg'
														"
														@error="$event.target.src = basedomainURL + '/Portals/Image/nouser1.png'"
														v-tooltip.top="sn.full_name"
														v-if="sn.avatar && idx < 5"
														style="border:1px solid #ccc;width:20px;height:20px;"
													/>
													<Avatar v-if="!sn.avatar && idx < 5"
														shape="circle"
														v-tooltip.top="sn.full_name"
														v-bind:label="sn.avatar ? '' : (sn.last_name ?? '').substring(0, 1)"
														style="cursor: pointer;width:20px; height:20px;border:1px solid #ccc;"
														:style="{ background: bgColor[idx % 7] + '!important'}"
													/>
												</li>
												<li v-if="u.seens.length - 5 > 0">
													<div style=" background-color: #f8fafb; color: #aaa; text-align: center; border-radius: 50%; margin-left: 0; font-size: 10px; max-height: min-content; width: 20px;height: 20px;border: solid 1px rgba(0,0,0,0.1);display: flex;align-items: center;justify-content: center;">
														<span>+{{u.seens.length-2}}</span>
													</div>
												</li>
											</ul>
										</div>
									</div>
								</div>
								<div id="nodata" v-if="props.listMessage.length == 0">
									<div style="display: table; height: calc(100vh - 260px); width: 100%">
										<div style="text-align:center;display: table-cell; vertical-align: middle;margin: auto;width: 100%;">
											<img width="200" v-bind:src="basedomainURL + '/Portals/Image/29103-chat-bubbles-2.gif'" />
											<p style="color: #aaa; margin-top: 20px; font-size: 16px"><b>Hiện chưa có tin nhắn nào</b></p>
										</div>
									</div>
								</div>
							</div>							
							<div class="btn-goBottom">
								<Button icon="pi pi-chevron-down" 
									class="p-button-rounded"
									id="id-btn-goBottom" 
									@click="goBottomChat" 
									style="float: right;"
								/>
							</div>
						</div>
					</div>
				</div>
			</div>
			<div v-if="!IsCall" class="box-footer" style=" border-radius: 10px;" :style="IsReply ? 'z-index:100;' : ''">
				<div>
					<div v-if="IsReply" style="">
						<div class="reply-chat show-reply" style="padding: 10px;background-color: antiquewhite;border-radius: 10px;margin: 10px;">
							<div class="row">
								<div class="col-12 md:col-12 flex">
									<div class="col-11 content-reply">
										<font-awesome-icon icon="fa-solid fa-quote-right" style="font-size: 1rem; color: gray;" />
										<div style="display: inline-block" class="bind-chat-html ml-2" v-html="tinnhanreply.content_message"></div>
									</div>
									<div class="col-1 close-reply text-right" v-if="isEdit != true">
										<a @click="HuyReply()"><i class="pi pi-times"></i></a>
									</div>
								</div>
								<div class="col-12 md:col-12 flex name-date-reply">
									<span style="padding-left: 10px;color: #888;font-size: 12px;">
										{{tinnhanreply.user_chat_firstname + ', ' + 
											(moment(new Date(tinnhanreply.created_date)).format("DD/MM/YYYY") == moment(new Date()).format("DD/MM/YYYY") 
											? ("Hôm nay lúc " + moment(new Date(tinnhanreply.created_date)).format("HH:mm"))
											: moment(new Date(tinnhanreply.created_date)).format("DD/MM/YYYY"))
										}}
									</span>
								</div>
							</div>

						</div>
					</div>
					<div class="r-file" v-if="FileAttach.length>0">
						<h3 style="font-weight: bold;font-size: 16px;margin: 10px 0;color: #2196f3;">Danh sách file đã chọn</h3>
						<ul class="my-2">
							<li v-for="(f, idx) in FileAttach" :key="idx">
								<div class="r-fbox">
									<img width="32" v-bind:src="basedomainURL+'/Portals/Image/file/'+f.file_type+'.png'" />
									<b>{{f.file_name}}</b>
									<span>{{formatByte(f.file_size)}}</span>
									<div style="width:40px;position:absolute;top:-10px;right:-20px;">
										<a @click="removeFilesComment(FileAttach,idx)"><i style="font-size:20px" class="pi pi-times-circle"></i></a>
									</div>
								</div>
							</li>
						</ul>
					</div>
					<div class="msb-reply flex" style="background-color: #fff; border-top: solid 2px #9acffa;">
						<ul class="list-group autoUser autoUsercm" v-if="props.detailChat.Focuscm" tabindex="0" @keydown="handleKeyDowncm($event)">
							<li class="list-group-item" style="padding:5px" 
								v-for="(u, idx) in filterUsercm" :key="idx"
								@click="clickAddUsercm(u)" 
								@mouseover="$parent.focusedIndex = $index">
								<div style="display:flex">
									<img class="ava" width="40" height="40" 
										v-bind:src="u.avatar
													? basedomainURL + u.avatar
													: basedomainURL + '/Portals/Image/noimg.jpg'
												"
										@error="$event.target.src = basedomainURL + '/Portals/Image/nouser1.png'"
										v-if="u.avatar"
									/>
									<Avatar v-else
										class="avt-replace"
										size="large"
										shape="circle"
										v-bind:label="(u.last_name ?? '').substring(0, 1)"
										style="cursor: pointer;width: 40px; height: 40px;"
										:style="{ background: bgColor[idx%7] + '!important'}"
									/>
									<div style="margin-left:5px">
										<b style="padding:5px">{{u.full_name}}</b>
										<strong style="display:block;padding:0 5px;font-weight:500;font-size:12px;color:#aaa">{{u.position_name}}</strong>
										<span style="display:block;font-size:12px;color:#aaa;padding:0 5px">{{u.organization_name}}</span>
									</div>
								</div>
							</li>
						</ul>
						<div class="flex flex-1">
							<div style="flex: 1;">
								<Textarea class="scroll-width-thin div-comment" 
									style="height: 75px; padding: 0.5rem; overflow: auto; border: none; font-size: 15px; width: 100%; text-align: justify;" 
									id="noiDungChat" 
									v-focus
									v-model="noiDungChat.noiDung" 
									v-on:keypress="changeContent($event)"
									v-on:keydown.enter.exact.prevent="sendMS(0, noiDungChat)" 
									v-on:keydown.enter.shift.exact.prevent="noiDungChat.noiDung += '\n'" 
									placeholder="Nhập nội dung tin nhắn..."
								/>
							</div>
						</div>
						<div v-if="isEdit!=true" class="showiconchat flex" style="width: 15rem;align-items: center;">
							<button class="btn-icon-chat" data-toggle="dropdown" v-tooltip.top="'Sticker'" @click="showEmoji($event, 1)" style="right:150px;color:#ff9900">
								<font-awesome-icon icon="fa-solid fa-face-smile" />
							</button>
							<button class="btn-icon-chat" style="right:100px;color:#777" @click="chooseFile('imageChat')" v-tooltip.top="'Ảnh'">
								<font-awesome-icon icon="fa-solid fa-image" />
							</button>
							<input class="hidden" id="imageChat" type="file" multiple="true" accept="image/*" @change="PutFileUpload" />
							<button class="btn-icon-chat" style="right:50px;color:#777" @click="chooseFile('fileUpChat')" v-tooltip.top="'File đính kèm'">
								<font-awesome-icon icon="fa-solid fa-paperclip" />
							</button>
							<input class="hidden" id="fileUpChat" type="file" multiple="true" accept="*" @change="PutFileUpload" />
							<button class="btn-icon-chat" style="right:0;" v-tooltip.top="'Gửi'" :disabled="(noiDungChat==null || noiDungChat.noiDung=='') && FileAttach.length == 0" @click="sendMS(0)">
								<font-awesome-icon icon="fa-solid fa-paper-plane" />
							</button>
						</div>
						<div v-if="isEdit==true" class="showiconchat flex" style="width: 10rem;align-items: center;">
							<button class="btn-icon-chat" data-toggle="dropdown" v-tooltip.top="'Sticker'" @click="showEmoji($event, 1)" style="right:100px;color:#ff9900">
								<font-awesome-icon icon="fa-solid fa-face-smile" />
							</button>
							<button class="btn-icon-chat" style="right:50px;" v-tooltip.top="'Gửi'" :disabled="(noiDungChat==null || noiDungChat.noiDung=='') && FileAttach.length == 0" @click="sendMS(0, noiDungChat)">
								<font-awesome-icon icon="fa-solid fa-paper-plane" />
							</button>
							<button class="btn-icon-chat" style="right:0;" v-tooltip.top="'Hủy'" @click="cancelEditMessage(noiDungChat)">
								<i class="pi pi-times-circle" style="font-size: 1.75rem; color:red; font-weight: bold;"></i> 
							</button>
						</div>
					</div>
				</div>
			</div>
			<!-- <DetailBoxChat 
				:key="componentKeyChat"
				:detailChat="props.detailChat"
				:listMessage="props.listMessage"
				:listMember="props.listMember"
				:funcCallSocket="callSocket"
				:funcCallUpdate="loadListChatGroup"
			/> -->
		</div>
		<div v-if="props.detailChat.IsInfoChat && !IsCall" class="main-right" style="background-color: #fff !important;">
			<div class="scroll-outer" style="min-height: calc(100vh - 60px); max-height: calc(100vh - 57px); overflow-y: auto;">
				<div class="scroll-inner">
					<div class="container-tab m-0">
						<ul class="nav nav-tabs chatinfotab" style="background-color: #fff;">
							<li style="width: 100%;">
								<a class="format_center" data-toggle="tab" data-target="#props.detailChat" style="cursor: default;padding: 8px; width: 100%; text-align: center; justify-content: left; height: 62px; font-weight: 700; color: #264061 !important; border-bottom: solid 5px #edf0f2; ">
									<h4 class="m-0 format_center" style="font-size:1.25rem;">
										<i class="pi pi-info-circle" style="font-size: 16px; padding: 5px;"></i> 
										<span> Thông tin hội thoại</span>
									</h4>
								</a>
							</li>
						</ul>
						<div style="max-height: calc(100vh - 120px);overflow-y:auto;" v-if="openResultSearchMsg">
							<div class="" style="padding-bottom: 0.75rem;border-bottom: 1px solid #e9e9e9;">
								<h3 class="m-2">Kết quản tìm kiếm</h3>
								<span class="p-2">Danh sách kết quản phù hợp trong cuộc hội thoại</span>
							</div>
							<div class="" v-if="listMsg_Search.length > 0">
								<div class="p-2 pt-3 font-bold" style="background-color:#e9ecef;">Tin nhắn</div>
								<div class="list-msg-search">
									<div class="flex line-msg-search" v-for="(msg, index) in listMsg_Search" @click="goToPositionMsg(msg)" :key="index">
										<div class="w-3rem pt-2 flex" style="justify-content:center;line-height:1.5;position: relative;height: 58px;">
											<img class="ava" width="48" height="48" 
												v-bind:src="msg.user_create_avatar
															? basedomainURL + msg.user_create_avatar
															: basedomainURL + '/Portals/Image/nouser1.png'
														"
												@error="$event.target.src = basedomainURL + '/Portals/Image/nouser1.png'"
												v-if="msg.user_create_avatar"
												style="border:1px solid #ccc;"
											/>
											<Avatar v-else
												class="avt-replace"
												size="large"
												shape="circle"
												v-bind:label="(msg.lastname ?? '').substring(0, 1)"
												style="cursor: pointer;width: 48px; height: 48px;border:1px solid #ccc;"
												:style="{ background: bgColor[index % 7] + '!important'}"
											/>
										</div>
										<div class="flex pl-2 py-0" style="width:calc(100% - 3rem);flex-direction: column;justify-content: center;">
											<div class="col-12 pb-1 pr-0 flex">
												<div class="flex-1" 
													style="width: calc(100% - 4rem); display: inline-grid; word-break: break-word; background: transparent;"
												>
													<span class="font-bold chatgroup-name">
														{{msg.user_chat_name}}
													</span>
												</div>
											</div>
											<div class="col-12 pt-1 pr-0 flex">
												<div class="info-law flex-1" style="width: calc(100% - 6rem); display: inline-grid; word-break: break-word; background: transparent;">
													<span class="mr-2" style="white-space: nowrap; overflow: hidden; text-overflow: ellipsis;">
														<span v-html="msg.content_message 
																	? (msg.content_message.includes('<br/>') ? msg.content_message.substring(0, msg.content_message.indexOf('<br/>')) : msg.content_message) 
																	: ''"
														>
														</span>													
													</span>
												</div>
												<div class="info-law w-6rem" style="justify-content:center;">
													<div class="btn-download-law" style="text-align: right;">
														<span class="" style="font-size:13px;">
															{{ msg.created_date 
																? (Math.floor(Math.abs(new Date(msg.date_now) - new Date(msg.created_date)) / 3600000) >= 24
																	? moment(new Date(msg.created_date)).format("HH:mm DD/MM") 
																	: (Math.floor(Math.abs(new Date(msg.date_now) - new Date(msg.created_date)) / 3600000) < 1
																		? (Math.floor(Math.abs(new Date(msg.date_now) - new Date(msg.created_date)) / 60000) > 1
																			? (Math.floor(Math.abs(new Date(msg.date_now) - new Date(msg.created_date)) / 60000) + ' phút trước')
																			: 'vừa xong')
																		: (Math.floor(Math.abs(new Date(msg.date_now) - new Date(msg.created_date)) / 3600000) + ' giờ trước')
																		)
																	)
																: ''
															}}
														</span>
													</div>
												</div>
											</div>
										</div>	
									</div>
								</div>
							</div>
							<div class="align-items-center justify-content-center p-4 text-center" v-else>
								<img src="../../assets/background/nodata.png" height="144" />
								<h4 class="m-1 font-italic">Không tìm thấy tin nhắn</h4>
							</div>
						</div>
						<div style="max-height: calc(100vh - 120px);overflow-y:auto;" v-else>
							<div class="tab-content" style="background-color: #fff;">
								<div id="props.detailChat" class="tab-pane fade in active">
									<div class="row scrollParentlist my-3" style="overflow-anchor: none; overflow-x: hidden; overflow-y: auto; max-height: 250px; min-height: unset;">
										<div style="width: 100%;">
											<div class="card box-info m-0" style="border:none; box-shadow: none;">
												<div class="card-body">
													<div class="media">
														<div class="media-body text-center">
															<div class="acc-info mb-2">
																<img class="ava" width="48" height="48" 
																	v-bind:src="props.detailChat.avatar_group
																				? basedomainURL + props.detailChat.avatar_group
																				: basedomainURL + '/Portals/Image/image_group_user.jpg'
																			"
																	v-if="props.detailChat.avatar_group || props.detailChat.is_group_chat == true"
																	@error="$event.target.src = basedomainURL + '/Portals/Image/image_group_user.jpg'"
																	style="border: 1px solid #ccc;"
																/>
																<Avatar v-else
																	class="avt-replace"
																	size="large"
																	shape="circle"
																	v-bind:label="(props.detailChat.chat_group_lastname ?? '').substring(0, 1)"
																	style="cursor: pointer;width: 48px; height: 48px;border: 1px solid #ccc;"
																	:style="{ background: bgColor[0] + '!important'}"
																/>
															</div>
															<div class="info">
																<p v-if="props.detailChat.is_group_chat" class="font-bold" style="margin:0; font-size: 18px;">{{props.detailChat.chat_group_name}}</p>
																<p v-else class="font-bold" style="margin:0; font-size: 18px;">{{props.detailChat.user_chat_name || ''}}</p>
															</div>
														</div>
													</div>
												</div>
											</div>
										</div>
									</div>
								</div>
							</div>						
							<Accordion class="info-group-chat" 
								:multiple="true" :activeIndex="listActiveTabInfoChat">
								<AccordionTab>
									<template #header>
										<div class="flex-1">
											<i class="pi pi-images" style="font-size: 16px; padding: 5px;"></i> 
											<span style="font-size: 16px;">Ảnh</span>
										</div>
										<Badge class="ml-2 mr-3" :value="tailieus.length > 0 ? tailieus[0].length : 0" 
											severity="danger"  :class="tailieus.length > 0 && tailieus[0].length > 0 ? 'custom-default' : 'custom-default'"></Badge>
									</template>
									<div class="tailieu-chat">
										<div style="min-height: unset; max-height: 320px !important; overflow-y: auto; ">
											<div class="r-file" v-if="tailieus.length > 0 && tailieus[0].length>0" style="padding:0">
												<ul class="my-2" style="display:grid; grid-template-columns: repeat(3, 33%);">
													<li v-for="(f, idxFile) in tailieus[0]" :key="idxFile" class="format_center p-0 position-relative items-file items-file-img" style="width: auto;">
														<div class="r-fbox image_file_chat" :class="{'p-2' : f.is_image!='1'}">
															<!-- <a @click="openFile(f,f.file_path)" v-if="f.is_image!='1' && f.file_type!='pdf'">
																<img width="32" v-bind:src="basedomainURL+'/Portals/Image/file/'+f.file_type+'.png'" v-if="f.is_image!='1' && f.file_type!='pdf'" style="width: 50% !important; height: 50% !important;" />
															</a> -->
															<Image v-if="f.is_image=='1'"
																class="flex"
																:src="basedomainURL+f.file_path"
																style="width: 100%; height: 100%; object-fit: contain;"
																preview
															/>
															<!-- <a @click="openFile(f,f.file_path)" v-if="f.is_image!='1' && f.file_type!='pdf'" style="color: inherit; width: 50% !important; height: 50% !important;">
																<b>{{f.file_name}}</b>
															</a>
															<span style="font-size:12px;" v-if="f.is_image!='1' && f.file_type!='pdf'">{{formatByte(f.file_size)}}</span> -->
															<div class="btn-group" role="group" style="display: none;position: absolute; top: 0; right: 0;">
																<Button
																	type="button"
																	class="btn cur-p no-after btn-mes-func icon-func-file"
																	icon="pi pi-ellipsis-h"
																	@click="toogleFuncFiles($event, f)"
																	aria-haspopup="true"
																	aria-controls="overlay_panelFuncFiles"
																	style="padding:0;"
																/>
															</div>
														</div>
													</li>
												</ul>
											</div>
											<div class="px-2 py-3 text-center" v-else>
												<span class="font-bold" style="font-size:1rem;">Chưa có file ảnh</span>
											</div>
										</div>
									</div>
								</AccordionTab>
								<AccordionTab>
									<template #header>
										<div class="flex-1">
											<i class="pi pi-folder-open" style="font-size: 16px; padding:5px;"></i> 
											<span style="font-size: 16px;">Tài liệu</span>	
										</div>
										<Badge class="ml-2 mr-3" :value="tailieus.length > 0 ? tailieus[1].length : 0" 
											severity="danger" :class="tailieus.length > 0 && tailieus[1].length > 0 ? 'custom-default' : 'custom-default'"></Badge>
									</template>
									<div class="tailieu-chat">
										<div style="min-height: unset; max-height: 315px !important; overflow-y: auto; ">
											<div class="r-file" v-if="tailieus.length > 0 && tailieus[1].length>0" style="padding:0">
												<ul class="my-2" style="display:grid; grid-template-columns: repeat(1, 100%);">
													<li v-for="(f, idxFile) in tailieus[1]" :key="idxFile" class="format_center p-0 position-relative items-file" style="width: auto;">
														<div class="r-fbox image_file_chat flex px-0" :class="{'p-2' : f.is_image!='1'}" style=" white-space: nowrap; overflow: hidden; text-overflow: ellipsis;">
															<a class="flex" @click="showfile(f)" v-if="f.is_image!='1'">
																<img width="32" v-bind:src="basedomainURL+'/Portals/Image/file/'+f.file_type+'.png'" v-if="f.is_image!='1'" style="width: 50px !important; height: 50px !important;" />
															</a>
															<div class="flex flex-1 ml-2" style="flex-direction:column; text-align:left; justify-content:center;width: calc(100% - 90px);">
																<a @click="showfile(f)" v-if="f.is_image!='1'" style="width: 95% !important; height: 50% !important;">
																	<b>{{f.file_name}}</b>
																</a>
																<!-- <a class="viewimg" v-bind:href="basedomainURL+f.file_path" data-fancybox :data-caption="f.file_name"></a> -->
																<span v-if="f.is_image!='1'">{{formatByte(f.file_size)}}</span>
															</div>
															<div class="btn-group" role="group" style="display: none;">
																<Button
																	type="button"
																	class="btn cur-p no-after btn-mes-func"
																	icon="pi pi-ellipsis-h"
																	@click="toogleFuncFiles($event, f)"
																	aria-haspopup="true"
																	aria-controls="overlay_panelFuncFiles"
																/>
															</div>
														</div>
													</li>
												</ul>
											</div>
											<div class="px-2 py-3 text-center" v-else>
												<span class="font-bold" style="font-size:1rem;">Chưa có file tài liệu</span>
											</div>
										</div>
									</div>
								</AccordionTab>
								<AccordionTab>
									<template #header>
										<div class="flex-1">
											<i class="pi pi-youtube" style="font-size: 16px; padding: 5px;"></i> 
											<span style="font-size: 16px;">Video/Audio</span>
										</div>
										<Badge class="ml-2 mr-3" :value="tailieus.length > 0 ? tailieus[3].length : 0" 
											severity="danger"  :class="tailieus.length > 0 && tailieus[3].length > 0 ? 'custom-default' : 'custom-default'"></Badge>
									</template>
									<div class="tailieu-chat">
										<div style="min-height: unset; max-height: 320px !important; overflow-y: auto; ">
											<div class="r-file" v-if="tailieus.length > 0 && tailieus[3].length>0" style="padding:0">
												<ul class="my-2" style="display:grid; grid-template-columns: repeat(3, 33%);">
													<li v-for="(f, idxFile) in tailieus[3]" :key="idxFile" class="format_center p-0 position-relative items-file items-file-img" style="width: auto;">
														<div class="r-fbox image_file_chat" :class="{'p-2' : f.is_image!='1'}">
															<a @click="openFile(f,f.file_path)" v-if="f.is_image!='1' && f.file_type!='pdf'">
																<img width="32" v-bind:src="basedomainURL+'/Portals/Image/file/'+f.file_type+'.png'" v-if="f.is_image!='1' && f.file_type!='pdf'" style="width: 50% !important; height: 50% !important;" />
															</a>
															<a @click="openFile(f,f.file_path)" v-if="f.is_image!='1' && f.file_type!='pdf'" style="color: inherit; width: 50% !important; height: 50% !important;">
																<b>{{f.file_name}}</b>
															</a>
															<span style="font-size:12px;" v-if="f.is_image!='1' && f.file_type!='pdf'">{{formatByte(f.file_size)}}</span>
															<div class="btn-group" role="group" style="display: none;position: absolute; top: 0; right: 0;">
																<Button
																	type="button"
																	class="btn cur-p no-after btn-mes-func icon-func-file"
																	icon="pi pi-ellipsis-h"
																	@click="toogleFuncFiles($event, f)"
																	aria-haspopup="true"
																	aria-controls="overlay_panelFuncFiles"
																	style="padding:0;"
																/>
															</div>
														</div>
													</li>
												</ul>
											</div>
											<div class="px-2 py-3 text-center" v-else>
												<span class="font-bold" style="font-size:1rem;">Chưa có file video/audio</span>
											</div>
										</div>
									</div>
								</AccordionTab>
								<AccordionTab>
									<template #header>
										<div class="flex-1">
											<i class="pi pi-link" style="font-size: 16px; padding: 5px;"></i> 
											<span style="font-size: 16px;">Links</span>
										</div>
										<Badge class="ml-2 mr-3" :value="tailieus.length > 0 ? tailieus[2].length : 0" 
											severity="danger"  :class="tailieus.length > 0 && tailieus[2].length > 0 ? 'custom-default' : 'custom-default'"></Badge>
									</template>
									<div class="tailieu-chat">
										<div style="min-height: unset; max-height: 320px !important; overflow-y: auto; ">
											<ul class="cv-ul-menubar" v-if="tailieus.length > 0 && tailieus[2].length>0">
												<li class="cv-li-menubar" v-for="(f, idxLink) in tailieus[2]" :key="idxLink">
													<a class="p-2" style="display:flex; flex:1;" :href="f.content_message" target="_blank">
														<div class="format_center p-2" style="position: relative; border: solid 1px #e5e5e5;">
															<i class="pi pi-link p-10"></i>
														</div>
														<div class="format_center ml-2" style="white-space: nowrap; overflow: hidden; text-overflow: ellipsis;">
															<div style="white-space: nowrap; overflow: hidden; text-overflow: ellipsis;">
																<span v-html="f.content_message"></span>
															</div>
														</div>
													</a>
												</li>
											</ul>
											<div class="px-2 py-3 text-center" v-else>
												<span class="font-bold" style="font-size:1rem;">Chưa có link</span>
											</div>
										</div>
									</div>
								</AccordionTab>
								<AccordionTab :disabled="true">
									<template #header>
										<i class="pi pi-cog" style="font-size: 16px; padding: 5px;"></i> 
										<span style="font-size: 16px;">Cài đặt khác</span>
									</template>
									<div class="tailieu-chat">
										<div style="min-height: unset; max-height: 320px !important; overflow-y: auto; margin-bottom:10px;">
											<ul class="cv-ul-menubar">
												<!-- <li class="cv-li-menubar"
													:class="props.detailChat.is_notify ? 'sao' : 'p-2'" 
													@click="Active_Notify(props.detailChat)">
													<div style="display:flex; flex:1;">
														<div class="format_center" style="position: relative;">
															<i class="pi pi-bell p-1 pl-2" style="font-size: 16px; "></i>
														</div>
														<div class="format_center ml-1" style="white-space: nowrap; overflow: hidden; text-overflow: ellipsis;">
															<div style="white-space: nowrap; overflow: hidden; text-overflow: ellipsis;">
																<span>{{props.detailChat.is_notify ? 'Tắt thông báo' : 'Bật thông báo'}}</span>
															</div>
														</div>
													</div>
												</li>
												<li v-if="!props.detailChat.is_group_chat" -->
												<li class="cv-li-menubar sao" 
													@click="Remove_Message(props.detailChat)">
													<div style="display:flex; flex:1;">
														<div class="format_center" style="position: relative;">
															<i class="pi pi-trash p-1 pl-2" style="font-size: 16px; "></i>
														</div>
														<div class="format_center ml-1" style="white-space: nowrap; overflow: hidden; text-overflow: ellipsis;">
															<div style="white-space: nowrap; overflow: hidden; text-overflow: ellipsis;">
																<span>Xóa lịch sử trò chuyện</span>
															</div>
														</div>
													</div>
												</li>
												<li v-if="(props.detailChat.is_group_chat && props.detailChat.created_by === store.getters.user.user_id)" 
													class="cv-li-menubar sao" 
													@click="Del_GroupChat(props.detailChat)">
													<div style="display:flex; flex:1;">
														<div class="format_center" style="position: relative;">
															<i class="pi pi-trash p-1 pl-2" style="font-size: 16px; "></i>
														</div>
														<div class="format_center ml-1" style="white-space: nowrap; overflow: hidden; text-overflow: ellipsis;">
															<div style="white-space: nowrap; overflow: hidden; text-overflow: ellipsis;">
																<span>Xóa nhóm chat</span>
															</div>
														</div>
													</div>
												</li>
												<li v-if="props.detailChat.is_group_chat" 
													class="cv-li-menubar sao" 
													@click="Out_GroupChat(props.detailChat, 1)">
													<div style="display:flex; flex:1;">
														<div class="format_center" style="position: relative;">
															<i class="pi pi-sign-out p-1 pl-2" style="font-size: 16px; "></i>
														</div>
														<div class="format_center ml-1" style="white-space: nowrap; overflow: hidden; text-overflow: ellipsis;">
															<div style="white-space: nowrap; overflow: hidden; text-overflow: ellipsis;">
																<span>Rời nhóm</span>
															</div>
														</div>
													</div>
												</li>
											</ul>
										</div>
									</div>
								</AccordionTab>
							</Accordion>
						</div>
					</div>
				</div>
			</div>
		</div>
		<!-- panel list icon -->
		<OverlayPanel
			class="p-0 panel_emote_chat"
			ref="panelEmoij4"
			appendTo="body"
			:showCloseIcon="false"
			id="overlay_panelEmoij4"
		>
			<VuemojiPicker @emojiClick="handleEmojiClick" />
		</OverlayPanel>
		<!-- panel menu message -->
		<OverlayPanel
			ref="funcMesChat"
			appendTo="body"
			class="p-0 m-0 panelFuncMes"
			:showCloseIcon="false"
			id="overlay_panelFuncMes"
			style="width: fit-content;"
		>
			<div>
				<ul class="ul-func-mes m-0" style="width: 10rem;">
					<li class="px-2 py-2" v-if="mesFuncClick.IsMe && mesFuncClick.type_message == 0">
						<a @click="EditMessage(mesFuncClick)" class="d-b td-n">
							<!-- <i class="pi pi-pencil"></i>  -->
							<font-awesome-icon icon="fa-solid fa-pencil" />
							<span class="ml-1"> Chỉnh sửa</span>
						</a>
					</li>
					<li class="px-2 py-2"> <a @click="Reply(mesFuncClick)" class="d-b td-n">
						<!-- <i class="las la-quote-right "></i>  -->
						<font-awesome-icon icon="fa-solid fa-quote-right" />
						<span class="ml-1"> Trả lời</span></a>
					</li>
					<li class="px-2 py-2"> <a @click="ShareMsg(mesFuncClick, 'overlay')" class="d-b td-n">
						<!-- <i class="las la-share"></i>  -->
						<font-awesome-icon icon="fa-solid fa-share" /> 
						<span class="ml-1"> Chia sẻ</span></a>
					</li>
					<li class="px-2 py-2" v-if="mesFuncClick.type_message == 1 || mesFuncClick.type_message == 2 || mesFuncClick.type_message == 3 || mesFuncClick.type_message == 4"> 
						<a @click="openFile(mesFuncClick)" class="d-b td-n">
						<i class="pi pi-download"></i>
						<span class="ml-1"> Tải xuống</span></a>
					</li>
					<li v-if="mesFuncClick.IsMe" role="separator" class="divider"></li>
					<li class="px-2 py-2" v-if="props.detailChat.isdelcomment || mesFuncClick.IsMe"> 
						<a @click="Del_Message(mesFuncClick,mesFuncClick.index)" class="d-b td-n" style="color:red;">
							<i class="pi pi-trash"></i> 
							<!-- <font-awesome-icon icon="fa-solid fa-trash-o" />  -->
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
			style="width: fit-content;"
		>
			<div>
				<ul class="ul-func-mes m-0" style="width: 10rem; padding:0; list-style-type: none;">
					<li class="px-2 py-2">
						<a @click="openFile(fileFuncClick,fileFuncClick.file_path)" class="d-b td-n">
							<i class="pi pi-download"></i>
							<span class="ml-1"> Tải xuống</span>
						</a>
					</li>
				</ul>
			</div>
		</OverlayPanel>
		<!-- view file -->
		<Sidebar v-model:visible="displayModalIframeDoc" 
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
	</div>
	<!-- Dialog edit group chat -->
	<Dialog
		:header="headerChat"
		v-model:visible="displayChat"
		:autoZIndex="true"
		:modal="true"
		style="z-index: 1000"
		:style="typeGroupChat == 1 ? 'width:50vw;' : 'width:35vw;'"
	>
		<!-- <form @submit.prevent="saveGroupChat(!(v$.chat_group_name.required.$invalid && v$.chat_group_name.required.$invalid))"> -->
		<form @submit.prevent="">
			<div class="grid formgrid m-2">
				<div class="col-12 md:col-12 flex mb-2" v-if="typeGroupChat == 1">
					<div class="flex w-7rem">
						<div class="inputanh relative flex" style="margin: 0 auto; width:100%; height:auto;">
							<img
								@click="chooseImage('imageUser')"
								id="groupChatAvt"
								v-bind:src="
								chat.avatar_group
									? basedomainURL + chat.avatar_group
									: basedomainURL + '/Portals/Image/image_group_user.jpg'
								"
							/>
							<Button
								v-if="chat.avatar_group || isDisplayAvt"
								style="width: 1.5rem; height: 1.5rem"
								icon="pi pi-times"
								@click="delAvatar"
								class="p-button-rounded absolute top-0 right-0 cursor-pointer"
							/>
							<input
								id="imageUser"
								type="file"
								accept="image/*"
								@change="handleFileUpload"
								style="display: none;"
							/>
						</div>
					</div>
					<div class="flex ml-3" style="width:calc(100% - 7rem);">
						<div class="col-12 md:col-12 flex">
							<div class="col-12 md:col-12 p-0 m-0 flex" style="flex-direction:column;">
								<label class="field col-12 p-0 text-left flex" style="align-items:center;">
									{{'Tên nhóm'}}
									<span class="redsao pl-1"> (*)</span>
								</label>
								<Textarea
									v-model="chat.chat_group_name"
									spellcheck="false"
									placeholder="Nhập tên nhóm..."
									class="col-12 ip36"
									rows="1"
									autoResize
									autofocus
									:class="{ 'p-invalid': v$.chat_group_name.$invalid && submitted }"
									style="padding:0.5rem;"
								/>
							</div>
						</div>
						<small class="col-12 p-error mt-2"
							v-if="(v$.chat_group_name.required.$invalid && submitted) || v$.chat_group_name.required.$pending.$response"
						>
							<div class="col-12 md:col-12 flex">
								<!-- <label class="col-2 text-left"></label> -->
								<span class="col-12 p-0">
									{{
										v$.chat_group_name.required.$message
											.replace("Value", "Tên nhóm chat")
											.replace("is required", "không được để trống")
									}}
								</span>
							</div>
						</small>
						<small class="col-12 p-error mt-2"
							v-if="(v$.chat_group_name.maxLength.$invalid && submitted) || v$.chat_group_name.maxLength.$pending.$response"
						>
							<div class="col-12 md:col-12 flex">
								<!-- <label class="col-2 text-left"></label> -->
								<span class="col-12 p-0">
									{{
										v$.chat_group_name.maxLength.$message.replace(
											"The maximum length allowed is",
											"Tên nhóm chat không được vượt quá"
										)
									}}
									ký tự
								</span>
							</div>
						</small>
					</div>
				</div>
				<div class="field col-12 md:col-12 p-0 flex" v-if="typeGroupChat == 1"></div>
				<div class="col-12 md:col-12 flex p-0">
					<div class="col-6" v-if="typeGroupChat == 1">
						<div class="flex">
							<label class="field col-12 p-0 font-bold text-left flex" style="align-items:center;">
								Thành viên trong nhóm
							</label>
						</div>
						<div class="flex">
							<div class="ip-us" tabindex="-1" style="overflow-x: hidden; overflow-y: auto; height: calc(100vh - 30rem); width: -webkit-fill-available;">
								<ul class="my-0" style="float: left; padding: 0; width: 100%;" v-if="props.listMember.length > 0">
									<li style="width: 100%; margin: 5px 0; float: left; list-style-type: none " 
										v-for="(m, index) in props.listMember" :key="index"
									>
										<div class="label-chat btn-info-user-chat" 
											style="width: 100%; white-space: unset; display: inline-flex; justify-content: space-between;"
											:style="m.user_join == chat.created_by ? 'border:solid 2px orange;' : ''"
										>
											<div class="flex" style="width: 100%;align-items: center;">
												<img class="ava" width="32" height="32" 
													v-bind:src="m.avatar
																? basedomainURL + m.avatar
																: basedomainURL + '/Portals/Image/noimg.jpg'
															"
													@error="$event.target.src = basedomainURL + '/Portals/Image/nouser1.png'"
													v-if="m.avatar"
												/>
												<Avatar v-else
													class="avt-replace"
													shape="circle"
													v-bind:label="(m.last_name ?? '').substring(0, 1)"
													style="cursor: pointer;width: 32px; height: 32px; border: 1px solid #e7e7e7;"
													:style="{ background: bgColor[index%7] + '!important'}"
												/>
												<strong style="padding: 7px; font-size: 14px; font-weight: 500;">{{m.full_name}}</strong>
											</div>
											<i v-if="(m.user_join != chat.created_by && chat.is_captain) || m.user_join == null"
												v-tooltip.top="'Rời khỏi nhóm'" 
												class="pi pi-times flex px-2" style="color:#fff;align-items: center;" 
												@click="removeUser(props.listMember,index)">
											</i>
										</div>
									</li>
								</ul>
							</div>
						</div>
					</div>
					<div :class="typeGroupChat == 1 ? 'col-6' : 'col-12 p-0'">
						<div class="flex" style="justify-content: space-between;">
							<label class="field font-bold">
								Danh sách người dùng
								<a v-if="chat.is_captain" 
									class="ml-1"
									@click="showusersModal(false,2)">
									<i class="pi pi-user-plus font-bold" v-tooltip.top="'Chọn thành viên'"></i>
								</a>
							</label>
							<label class="field font-bold">
								<a v-if="chat.chat_group_id && !chat.is_captain" 
									class="font-bold p-2 btn-out-group" 
									@click="Out_GroupChat(chat, 1)">
									<i class="pi pi-sign-out font-bold"></i>
									Rời khỏi nhóm
								</a>
							</label>
						</div>
						<div class="flex">
							<div class="" tabindex="-1" style="overflow-x: hidden; overflow-y: auto; height: calc(100vh - 30rem); width: -webkit-fill-available;">
								<div style=" border: solid 1px #ced4da; margin-bottom: 5px; border-radius: 5px;">
									<input class="ipautoUser" :class="chat.Focus" 
										v-model="chat.searchU" 
										style="width: 100%; border: none; background-color: transparent; padding: 7px 5px;" 
										placeholder="Tìm kiếm bằng tên" />
								</div>
								<div class="scrollParenttree" style="overflow-x: hidden; overflow-y: auto;">
									<ul class="list-group dataUser p-0 m-0" tabindex="0" style="max-height: calc(100vh - 32.75rem);">
										<div v-for="pb in departments" :key="pb.department_id" class="p-0">
											<div class="p-3" style="background-color: antiquewhite;" v-if="filterListUser(pb).length > 0">
												<span class="font-bold uppercase" style="color: #333;">{{pb.department_name}}</span>
											</div>
											<li class="list-group-item format_center"
												style="padding:0.5rem 0.25rem;" 
												v-for="(u, index) in filterListUser(pb)" :key="index"
												@click="clickAddUser(chat,props.listMember,u,'')">
												<div class="" style="display: -webkit-box; display: -webkit-inline-box; justify-content: flex-start; width: 100%;">
													<img class="ava" width="48" height="48" 
														v-bind:src="u.avatar
																	? basedomainURL + u.avatar
																	: basedomainURL + '/Portals/Image/noimg.jpg'
																"
														@error="$event.target.src = basedomainURL + '/Portals/Image/nouser1.png'"
														v-if="u.avatar"
													/>
													<Avatar v-else
														class="avt-replace"
														shape="circle"
														v-bind:label="(u.last_name ?? '').substring(0, 1)"
														style="cursor: pointer;width: 48px; height: 48px;"
														:style="{ background: bgColor[u.is_order % 7] + '!important', }"
													/>
													<div class="format_text" style="margin-left:5px;">
														<b style="padding:5px">{{u.full_name}}</b>
														<strong style="display:block;padding:3px 5px;font-weight:500;font-size:12px;color:#aaa">
															<span class="font-medium">
																{{ u.user_id != null ? u.user_id : '' }}
															</span>
															{{ u.position_name ? (' | ' + u.position_name) : '' }}
														</strong>
														<span style="display:block;font-size:12px;color:#aaa;padding:0 5px">{{u.department_name}}</span>
														<!-- <span style="display:block;font-size:12px;color:#aaa;padding:0 5px">{{u.organization_name}}</span> -->
													</div>
												</div>
											</li>
										</div>
									</ul>
								</div>
							</div>
						</div>
					</div>
				</div>
			</div>
		</form>
		<template #footer>
			<Button
				label="Hủy"
				icon="pi pi-times"
				@click="closeDialog()"
				class="p-button-text"
			/>

			<Button v-if="typeGroupChat == 1" label="Lưu" icon="pi pi-check" @click="saveGroupChat(!(v$.chat_group_name.required.$invalid && v$.chat_group_name.required.$invalid))" />
		</template>
	</Dialog>
	<!-- Dialog share message -->
	<Dialog
		:header="headerShareChat"
		v-model:visible="displayShareChat"
		:autoZIndex="true"
		:modal="true"
		style="z-index: 1000;width:50vw;"
	>
		<form>
			<div class="grid formgrid m-2">
				<div class="col-12 md:col-12 flex p-0">
					<div class="col-6">
						<div class="flex" style="flex-direction:column;">
							<div style=" border: solid 1px #ced4da; margin-bottom: 5px; border-radius: 5px;">
								<input class="ipautoUser" 
									v-model="share.searchU" 
									style="width: 100%; border: none; background-color: transparent; padding: 7px 5px;" 
									placeholder="Tìm kiếm hội thoại cần chia sẻ" />
							</div>
							<div class="" tabindex="-1" style="overflow-x: hidden; overflow-y: auto; height: calc(100vh - 40rem); width: -webkit-fill-available;">
								<div class="scrollParenttree" style="overflow-x: hidden; overflow-y: auto;">
									<ul class="list-group dataUser p-0 m-0" tabindex="0">
										<div class="p-0">
											<div class="p-3" style="background-color: antiquewhite;" v-if="filterListChatRecent().length > 0">
												<span class="font-bold uppercase" style="color: #333;">Trò chuyện gần đây</span>
											</div>											
											<li class="list-group-item format_center"
												style="padding:0.5rem 0.25rem;" 
												v-for="(u, index) in filterListChatRecent()" :key="index">
												<div class="" style="display: -webkit-box; display: -webkit-inline-box; justify-content: flex-start; width: 100%;display: flex; align-items: center;">
													<Checkbox class="mr-3" :binary="u.checkShare" v-model="u.checkShare" @change="clickAddShareMsg(u, 1)" />
													<img class="ava" width="48" height="48" 
														v-bind:src="u.avatar_group
																	? basedomainURL + u.avatar_group
																	: basedomainURL + '/Portals/Image/image_group_user.jpg'
																"
														@error="$event.target.src = basedomainURL + '/Portals/Image/image_group_user.jpg'"
														v-if="u.avatar_group"
													/>
													<Avatar v-else
														class="avt-replace"
														shape="circle"
														v-bind:label="(u.chat_group_lastname ?? '').substring(0, 1)"
														style="cursor: pointer;width: 48px; height: 48px; border: 1px solid #e7e7e7;"
														:style="{ background: bgColor[index%7] + '!important'}"
													/>
													<div class="label-2lines" style="margin-left:10px;">
														<b class="p-0">{{u.chat_group_name}}</b>
													</div>
												</div>
											</li>
											<div class="p-3" style="background-color: antiquewhite;" v-if="filterListUserAll().length > 0">
												<span class="font-bold uppercase" style="color: #333;">Danh sách người dùng</span>
											</div>											
											<li class="list-group-item format_center"
												style="padding:0.5rem 0.25rem;" 
												v-for="(u, index) in filterListUserAll()" :key="index">
												<div class="" style="display: -webkit-box; display: -webkit-inline-box; justify-content: flex-start; width: 100%;display: flex; align-items: center;">
													<Checkbox class="mr-3" :binary="u.checkShare" v-model="u.checkShare" @change="clickAddShareMsg(u, 2)" />
													<img class="ava" width="48" height="48" 
														v-bind:src="u.avatar
																	? basedomainURL + u.avatar
																	: basedomainURL + '/Portals/Image/noimg.jpg'
																"
														@error="$event.target.src = basedomainURL + '/Portals/Image/nouser1.png'"
														v-if="u.avatar"
													/>
													<Avatar v-else
														class="avt-replace"
														shape="circle"
														v-bind:label="(u.last_name ?? '').substring(0, 1)"
														style="cursor: pointer;width: 48px; height: 48px;"
														:style="{ background: bgColor[u.is_order % 7] + '!important', }"
													/>
													<div class="label-2lines" style="margin-left:10px;">
														<b class="p-0">{{u.full_name}}</b>
													</div>
												</div>
											</li>
										</div>
									</ul>
								</div>
							</div>
						</div>
					</div>
					<div class="col-6">
						<div class="flex">
							<label class="field col-12 p-0 font-bold text-left flex" style="align-items:center;">
								Đã chọn
							</label>
						</div>
						<div class="flex">
							<div class="ip-us" tabindex="-1" style="overflow-x: hidden; overflow-y: auto; height: calc(100vh - 40rem); width: -webkit-fill-available;">
								<ul class="my-0" style="float: left; padding: 0; width: 100%;">
									<li style="width: 100%; height: 36px; margin: 5px 0; float: left; list-style-type: none " 
										v-for="(m, index) in listChatShare" :key="index"
									>
										<div class="label-chat btn-info-user-chat" style="width: 100%; white-space: unset; display: inline-flex; justify-content: space-between;">
											<div class="flex" style="width: 100%;align-items: center;">
												<img class="ava" width="32" height="32" 
													v-bind:src="m.avatar
																? basedomainURL + m.avatar
																: basedomainURL + '/Portals/Image/noimg.jpg'
															"
													:style="m.user_join == chat.created_by ? 'border:solid 2px orange;': ''"
													@error="$event.target.src = basedomainURL + '/Portals/Image/nouser1.png'"
													v-if="m.avatar"
												/>
												<Avatar v-else
													class="avt-replace"
													shape="circle"
													v-bind:label="(m.last_name ?? '').substring(0, 1)"
													style="cursor: pointer;width: 32px; height: 32px;"
													:style="{ background: bgColor[index % 7] + '!important', }"
												/>
												<strong style="padding: 7px; font-size: 14px; font-weight: 500;">{{m.full_name}}</strong>
											</div>
										</div>
									</li>
								</ul>
							</div>
						</div>
					</div>
				</div>
				<div class="col-12 md:col-12 flex p-0" style="border-top: solid 1px rgba(0,0,0,0.1); min-height: 18rem;">
					<div class="col-12 py-3">
						<label class="font-bold" for="inputState">Nội dung chia sẻ</label>
						<div v-if="(msgShare.type_message === 1 && msgShare.FileAttach_Share.length > 0) || (msgShare.type_message === 2 && msgShare.FileAttach_Share.length > 0)" 	
							class="r-file pX-0">
							<ul class="p-0 mb-0">
								<li v-if="msgShare.type_message === 1 && msgShare.FileAttach_Share.length > 0" class="m-0 p-0">
									<div class="r-cm p-0 m-0">
										<div style="max-width: 300px;">
											<Image class="flex img-share"
												:src="basedomainURL + (msgShare.FileAttach_Share[0].file_path ||'/Portals/Image/noimg.jpg')"
												preview
											/>
										</div>
									</div>
								</li>
								<li v-else class="m-0" v-for="(f, idx) in msgShare.FileAttach_Share" :key="idx">
									<div class="r-fbox" v-if="msgShare.type_message === 2">
										<img width="32" v-bind:src="basedomainURL+'/Portals/Image/file/'+f.file_type+'.png'" />
										<b>{{f.file_name}}</b>
										<span>{{formatByte(f.file_size)}}</span>
									</div>
								</li>
							</ul>
						</div>
						<div class="msb-reply mt-2 flex">
							<Textarea class="scroll-width-thin div-comment" 
								style="resize: none; min-height: 75px; width: 100%; padding: 10px; overflow: auto; background-color: transparent; 
									border: solid 1px #bfbfbf; font-size: 15px; color: #000;
									border-radius: 10px;
									/*border-right: none; border-top-left-radius: 10px; border-bottom-left-radius: 10px;*/
								" 
								id="noiDungChat_Share" 
								autoResize
								v-focus
								v-model="msgShare.content_message" 
								placeholder="Nhập nội dung tin nhắn..." disabled>
							</Textarea>
							<!-- <div class="showiconchat">
								<button class="btn-icon-chat px-3" data-toggle="dropdown" v-tooltip.top="'Sticker'" @click="showEmoji($event, 1)" 
									style="right:0;color:#ff9900; border: 1px solid #d9d9d9; border-left: none; border-top-right-radius: 10px; border-bottom-right-radius: 10px;">
									<font-awesome-icon icon="fa-solid fa-face-smile" />
								</button>
							</div> -->
						</div>
					</div>
				</div>
			</div>
		</form>
		<template #footer>
			<Button
				label="Hủy"
				icon="pi pi-times"
				@click="closeDialog()"
				class="p-button-text"
			/>

			<Button label="Chia sẻ" icon="pi pi-send" @click="shareMesChat()" />
		</template>
	</Dialog>
	<!-- Dialog show info user chat -->
	<Dialog
		class="dialog-infouser"
		:header="'Thông tin'"
		v-model:visible="showModalInfoUser"
		:autoZIndex="true"
		:modal="true"
		style="z-index:1000;width:30vw;"
	>
		<form>
			<div class="grid formgrid m-2">
				<div class="col-12 md:col-12 mb-2">
					<div class="col-12" style="height: 130px; background-color: #00B0F0">
						<div class="format_center">
							<h4></h4>
						</div>
					</div>
					<div class="col-12">
						<div class="inputanh relative flex" style="margin: 0 auto;transform: translate(0%, -50%);">
							<img id="groupChatAvt"
								v-bind:src="
								infoUserChat.avatar
									? basedomainURL + infoUserChat.avatar
									: basedomainURL + '/Portals/Image/noimg.jpg'
								"
							/>
							<span class="online" style="bottom:0;right:10px;width:1.25rem;height:1.25rem;"></span>
						</div>
						<div class="text-center mb-2" style="margin-top: -2.5rem;">
							<span class="font-bold" style="font-size:1.25rem;">{{infoUserChat.full_name}}</span>
						</div>
					</div>
				</div>
				<div class="field col-12 md:col-12 p-0 flex"></div>
				<div class="col-12 md:col-12 field flex p-0">
					<div class="col-3 label-infouser">
						<label>Chức vụ</label>
					</div>
					<div class="col-9 content-infouser">
						<span>{{infoUserChat.position_name}}</span>
					</div>
				</div>
				<div class="col-12 md:col-12 field flex p-0">
					<div class="col-3 label-infouser">
						<label>Điện thoại</label>
					</div>
					<div class="col-9 content-infouser">
						<span>{{infoUserChat.phone}}</span>
					</div>
				</div>
				<div class="col-12 md:col-12 field flex p-0">
					<div class="col-3 label-infouser">
						<label>Email</label>
					</div>
					<div class="col-9 content-infouser">
						<span>{{infoUserChat.email}}</span>
					</div>
				</div>
				<div class="col-12 md:col-12 field flex p-0">
					<div class="col-3 label-infouser">
						<label>Ngày sinh</label>
					</div>
					<div class="col-9 content-infouser">
						<span>{{ infoUserChat.birthday ? moment(new Date(infoUserChat.birthday)).format("DD/MM/YYYY") : '' }}</span>
					</div>
				</div>
				<div class="col-12 md:col-12 field flex p-0">
					<div class="col-3 label-infouser">
						<label>Giới tính</label>
					</div>
					<div class="col-9 content-infouser">
						<span>{{infoUserChat.gender ? 'Nam' : 'Nữ'}}</span>
					</div>
				</div>
				<div class="col-12 md:col-12 field flex p-0">
					<div class="col-3 label-infouser">
						<label>Nhóm chung</label>
					</div>
					<div class="col-9 content-infouser">
						<span>{{infoUserChat.sameGroup}}</span>
					</div>
				</div>
			</div>
		</form>
		<template #footer>
			<Button
				label="Hủy"
				icon="pi pi-times"
				@click="closeDialogInfo()"
				class="p-button-text"
			/>
		</template>
	</Dialog>
	<!-- Dialog show user using stick for message -->
	<Dialog v-model:visible="displayStick"
		:dismissableMask="true"
		:modal="true"
		style="z-index:1000;"
		:header="'Tương tác'"
		class="dialog-stick"
	>
		<div style="background-color:#fff;padding:0 10px 10px;border-top: 1px solid #f5f5f5;">
			<TabView lazy :active-index="tabStickActive" @tab-change="changeTabStick">
				<TabPanel>
					<template #header>
						<span>Tất cả ({{emote_message.length}})</span>
					</template>
					<div class="sti-row">
						<div class="row-comment" v-for="(u, idx) in emote_message" :key="idx">
							<div style="display:flex; width:100%;">
								<div style="position: relative;" v-if="u.avatar">
									<img class="ava" width="48" height="48" 
										v-bind:src="u.avatar
													? basedomainURL + u.avatar
													: basedomainURL + '/Portals/Image/noimg.jpg'
												"
										@error="$event.target.src = basedomainURL + '/Portals/Image/nouser1.png'"
									/>
									<span style="position: absolute;width: 1rem;height: 1rem;bottom: 3px;left: 35px;">
										<img v-bind:src='basedomainURL + u.stick_file' style="height: 16px;" />
									</span>
								</div>
								<Avatar v-else
									class="avt-replace"
									size="large"
									shape="circle"
									v-bind:label="(u.last_name ?? '').substring(0, 1)"
									style="cursor: pointer;width: 48px; height: 48px;"
									:style="{ background: bgColor[idx%7] + '!important'}"
								/>
								<div style="margin-left:5px;width: -webkit-fill-available;">
									<div class="flex align-items-center" style="padding:5px">
										<b class="flex-1">{{u.full_name}}</b>
										<span class="r-cdate">{{moment(new Date(u.created_date)).format("HH:mm DD/MM/YYYY")}}</span>
									</div>
									<strong style="display:block;padding:0 5px;font-weight:500;font-size:12px;color:#aaa">{{u.position_name}}</strong>
									<span style="display:block;font-size:12px;color:#aaa;padding:0 5px">{{u.department_name}}</span>
								</div>
							</div>
						</div>
					</div>
				</TabPanel>
				<TabPanel v-for="(st, index) in emoteList" :key="index">
					<template #header>
						<img v-bind:src="basedomainURL + st.emote_file" 
							style="width: 32px !important; height: 32px !important; border-radius: 50%;" 
						/> 
						<span class="pl-1" v-if="st.sticks.length>0">({{st.sticks.length}})</span>
					</template>
					<div class="sti-row">
						<div class="row-comment" v-for="(u, idx) in st.sticks" :key="idx">
							<div style="display:flex; width:100%;">
								<img class="ava" width="48" height="48" 
									v-bind:src="u.avatar
												? basedomainURL + u.avatar
												: basedomainURL + '/Portals/Image/noimg.jpg'
											"
									@error="$event.target.src = basedomainURL + '/Portals/Image/nouser1.png'"
									v-if="u.avatar"
								/>
								<Avatar v-else
									class="avt-replace"
									size="large"
									shape="circle"
									v-bind:label="(u.last_name ?? '').substring(0, 1)"
									style="cursor: pointer;width: 48px; height: 48px;"
									:style="{ background: bgColor[idx%7] + '!important'}"
								/>
								<div style="margin-left:5px;width: -webkit-fill-available;">
									<div class="flex align-items-center" style="padding:5px;">
										<b class="flex-1">{{u.full_name}}</b>
										<span class="r-cdate">{{moment(new Date(u.created_date)).format("HH:mm DD/MM/YYYY")}}</span>
									</div>
									<strong style="display:block;padding:0 5px;font-weight:500;font-size:12px;color:#aaa">{{u.position_name}}</strong>
									<span style="display:block;font-size:12px;color:#aaa;padding:0 5px">{{u.department_name}}</span>
								</div>
							</div>
						</div>
					</div>
				</TabPanel>
			</TabView>
		</div>
	</Dialog>
	<!-- Dialog list user seen -->
	<Dialog v-model:visible="displaySeen"
		:dismissableMask="true"
		:modal="true"
		:header="'Danh sách người dùng đã xem'"
		class="dialog-stick"
		style="width:35rem;z-index:1000;"
	>
		<div style="background-color:#fff;padding:0 0 10px;border-top: 1px solid #f5f5f5;">
			<div class="sti-row">
				<div class="row-comment" v-for="(u, idx) in listUserSeen" :key="idx" style="border-top:1px solid #f3f3f3;">
					<div style="display:flex; width:100%; align-items: center;">
						<div style="position: relative;" v-if="u.avatar">
							<img class="ava" width="48" height="48" 
								v-bind:src="u.avatar
											? basedomainURL + u.avatar
											: basedomainURL + '/Portals/Image/noimg.jpg'
										"
								@error="$event.target.src = basedomainURL + '/Portals/Image/nouser1.png'"
							/>
						</div>
						<Avatar v-else
							class="avt-replace"
							size="large"
							shape="circle"
							v-bind:label="(u.last_name ?? '').substring(0, 1)"
							style="cursor: pointer;width: 48px; height: 48px;"
							:style="{ background: bgColor[idx%7] + '!important'}"
						/>
						<div style="margin-left:5px;width: -webkit-fill-available;">
							<div class="flex align-items-center" style="padding:5px">
								<b class="flex-1">{{u.full_name}}</b>
								<span class="r-cdate">{{u.last_seen_date != null ? moment(new Date(u.last_seen_date)).format("HH:mm DD/MM/YYYY") : ''}}</span>
							</div>
							<strong style="display:block;padding:0 5px;font-weight:500;font-size:12px;color:#aaa">{{u.position_name}}</strong>
							<span style="display:block;font-size:12px;color:#aaa;padding:0 5px">{{u.department_name}}</span>
						</div>
					</div>
				</div>
			</div>
		</div>
	</Dialog>
</template>
<style scoped>
	@import url(./stylechat.css);
</style>
<style lang="scss" scoped>
	::v-deep(.dialog-infouser) {
		.p-dialog-content {
			padding-bottom: 0;
		}
	}
	::v-deep(.image_preview_chat) {
		.p-image-preview-container img {
			width: 100%;
			height: 100%;
			object-fit: contain;
			border-radius: 20px 20px 0px 0px;
		}
		.p-image-preview-container:hover > .p-image-preview-indicator {
			border-radius: 20px 20px 0px 0px;
		}
	}
	::v-deep(.image_file_chat) {
		.p-image-preview-container img {
			width: 100%;
			height: 100%;
			object-fit: contain;
		}
	}
	::v-deep(.info-group-chat) {
		.p-accordion-content {
			padding: 0;
			border-bottom: none;
			border-left: none;
		}
		.p-accordion-header.p-highlight .p-accordion-header-link {
			background-color: #e9ecef !important;
		}
		.p-accordion-header.p-highlight.p-disabled {
			pointer-events: unset;
			user-select: unset;
			opacity: 1;
		}
		.p-accordion-header.p-highlight.p-disabled .p-accordion-header-link {
			background-color: #f8f9fa !important;
		}		
		.p-accordion-header-link {
			padding: 0.75rem 0.5rem;
			border-top-right-radius: 3px !important;
    		border-top-left-radius: 3px !important;
    		border-top: 1px solid #dee2e6 !important;
		}
		.p-accordion-toggle-icon {
			display: none;
		}
		.p-badge {
			min-width: 2rem;
			max-width: 3rem;
			// width: -webkit-fill-available;
			height: 2rem;
			font-size: 1rem;
			border-radius: 50%;
			display: flex;
    		align-items: center;
    		justify-content: center;
		}
		.p-badge.custom-default {
			background-color: #bbbbbb;
		}
	}
	::v-deep(.img-share) {
		img {
			width: 100%;
			height: 100%;
			object-fit: contain;
		}
	}
	::v-deep(.avt-replace) {
		.p-avatar-text {
			display: flex;
			width: inherit;
			align-items: center;
			justify-content: center;
			font-size: initial;
			text-transform: uppercase;
			color: #000;
		}
	}
</style>