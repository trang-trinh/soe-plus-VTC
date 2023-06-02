<script setup>
import { ref, inject, onMounted, watch } from "vue";
import { useToast } from "vue-toastification";
import { required, maxLength } from "@vuelidate/validators";
import { useVuelidate } from "@vuelidate/core";
import { useRouter, useRoute } from "vue-router";
import { VuemojiPicker } from "vuemoji-picker";
import moment from "moment";
import { useCookies } from "vue3-cookies";
import chatMessage from "../../components/chat/DetailChat.vue";
import { encr, change_unsigned, checkURL } from "../../util/function.js";
import { socketMethod } from "../../util/methodSocket";
import treeuser from "../../components/user/treeuser.vue";
const cryoptojs = inject("cryptojs");
//Khai báo
const route = useRoute();
const axios = inject("axios");
const store = inject("store");
const swal = inject("$swal");
const socket = inject("socket");
const router = inject("router");
// reload component
const componentKey = ref(0);
const forceRerender = () => {
  componentKey.value += 1;
};
const { cookies } = useCookies();
const showInfoChat = ref(true);
//Nơi nhận EMIT từ component
const emitter = inject("emitter");
emitter.on("emitData", (obj) => {
	switch (obj.type) {
		// case "loadListChatGroup":
		// 	loadDataGroupChat();
		// 	//detailChat.value = null;
		// 	if (obj.data != null) {
		// 		showInfoChat.value = obj.data.IsInfoChat;
		// 	}
		// 	forceRerender();
		// 	break;
		case "loadDetailChat":
			reloadDataChat();
			break;
		default:
			break;
	}
});
const loadListChatGroup = () => {
	loadDataGroupChat();
	forceRerender();
};
const options = ref({
  sort: "modified_date",
  SearchText: "",
  loading: true,
  TypeChat: -1,
  organization_id: store.getters.user.organization_id,
  user_key: store.getters.user.user_key,
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
const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};

const bgColor = ref([
    "#F8E69A", "#AFDFCF", "#F4B2A3", "#9A97EC", "#CAE2B0", "#8BCFFB", "#CCADD7"
]);

const chat = ref({
	chat_group_name: "",
	status: 1,
	organization_id: store.getters.user.organization_id,
});
const v$ = useVuelidate(rules, chat);
const basedomainURL = baseURL;
const baseUrlCheck = baseURL;
const toast = useToast();
const datalists = ref([]);
const layout = ref("list");
//Thêm log
const addLog = (log) => {
  axios.post(baseURL + "/api/Proc/AddLog", log, config);
};

//const countListChat = ref(0);
const chat_group_active = ref();

const loadDataGroupChat = (isGetRealtime) => {
	axios
      .post(
		baseUrlCheck + "api/chat/GetDataProc",
		{ 
			str: encr(JSON.stringify({
					proc: "chat_group_list",
					par: [
						{ par: "organization_id", va: store.getters.user.organization_id },
						{ par: "user_id ", va: store.getters.user.user_id },
						//{ par: "pageno", va: options.value.PageNo },
						//{ par: "pagesize", va: options.value.PageSize },
						{ par: "search", va: null },
						{ par: "type_chat", va: options.value.TypeChat },
						{ par: "sort", va: options.value.sort },
						//{ par: "chat_id_active", va: localStorage.getItem("ck_cgi") },
						{ par: "chat_id_active", va: cookies.get("ck_cgi") },
					],
				}), SecretKey, cryoptojs
			).toString()
		},
		config
      )
      .then((response) => {
		let data = JSON.parse(response.data.data);
		if (data[0].length > 0){
			data[0].forEach((el, idx) => {
				if (el.last_user_chat_name != null && el.last_user_chat_name.trim() != '') {
					el.last_user_firstname = el.last_user_chat_name.trim().lastIndexOf(" ") >= 0 ? el.last_user_chat_name.trim().substring(el.last_user_chat_name.trim().lastIndexOf(" ") + 1) : el.last_user_chat_name.trim();
					el.user_last_chat = el.last_user_chat_id == store.getters.user.user_id ? 'Bạn' : el.last_user_firstname;
				}
				el.is_order = idx;
			});
		}
		datalists.value = data[0];
		//countListChat.value = datalists.value.length;
		//options.value.loading = false;
		if(route.params.id != null){
          	//nhan thong bao tu notify
          	//localStorage.setItem("ck_cgi", route.params.id);
			cookies.set("ck_cgi", route.params.id);
        }
		else if (route.params.uid != null && route.params.typeid == 'dashboard') {
			let listPersonalChat = datalists.value.filter(x => x.is_group_chat != true && ((x.user_chat == route.params.uid && x.created_by == store.getters.user.user_id) || (x.created_by == route.params.uid && x.user_chat == store.getters.user.user_id)));
			if (listPersonalChat.length == 0){
				var userChoose = filterUser.value.filter(x => x.user_id == route.params.uid);
				if (userChoose.length > 0) {
					chat.value.chat_group_name = userChoose[0].full_name;
					chat.value.is_group_chat = false;
					chat.value.user_chat = userChoose[0].user_id;
					chat.value.avatar_group = userChoose[0].avatar;
					typeGroupChat.value = 0;
					saveGroupChat();
				}
			}
			else {
				if (cookies.get("ck_cgi") != null) {
					cookies.remove("ck_cgi");
				}
				showDetailChat(listPersonalChat[0]);
			}
		}
		// if (localStorage.getItem("ck_cgi") != null) {
		// 	let chatLocal = { chat_group_id: localStorage.getItem("ck_cgi") };
		// 	showDetailChat(chatLocal, isGetRealtime);
		// }
		if (cookies.get("ck_cgi") != null) {
			let chatLocal = { chat_group_id: cookies.get("ck_cgi") };
			showDetailChat(chatLocal, isGetRealtime);
		}
		else {
			detailChat.value = null;
		}
		if ((data[1] == null || (data[1] != null && data[1][0] != null && data[1][0].is_notify)) && isGetRealtime) {
			let audioNoti = new Audio(baseUrlCheck + "/Portals/FileChatSystem/pristine-sound.mp3");
			audioNoti.play();
		}
		var users = store.getters.userConnected;
		
		datalists.value.forEach((el, idx) => {
			if (el.is_group_chat != true && el.displayInList == 1) {				
				let userChat = el.user_chat == store.getters.user.user_id ? el.created_by : el.user_chat;
				if (users.findIndex(x => x.user_id == userChat && x.connected) >= 0) {
					el.Online = true;
				}
				else{
					el.Online = false;
				}
			}
			else {
				el.Online = false;
			}
		});
      })
	  .then(() => {
		var uids = datalists.value.filter(x => x.is_group_chat != true && x.displayInList == 1);
		//.map(x => (x.user_chat == store.getters.user.user_id ? x.created_by : x.user_chat));
		socket.emit('sendData', { event:"reloadListChat", uids: uids});
	  })
      .catch((error) => {
        //toast.error("Tải dữ liệu không thành công!");
        console.log("Tải dữ liệu không thành công!");
        //options.value.loading = false;
        addLog({
          title: "Lỗi Console loadData",
          controller: "SQLView.vue",
          logcontent: error.message,
          loai: 2,
        });
        if (error && error.status === 401) {
          swal.fire({
            title: "Thông báo",
            text: "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại!",
            icon: "error",
            confirmButtonText: "OK",
          });
          store.commit("gologout");
        }
      });
};

const detailChat = ref();
const listMessage = ref ([]);
const listMember = ref ([]);
const showDetailChat = (chatShow, isGetRealtime) => {
	//detailChat.value = null;
	chat_group_active.value = chatShow.chat_group_id;
	axios
		.post(
			baseUrlCheck + "api/chat/GetDataProc",
			{ 
				str: encr(JSON.stringify({
						proc: "chat_group_detail",
						par: [
							//{ par: "chat_group_id", va: chatShow.chat_group_id || localStorage.getItem("ck_cgi") },
							{ par: "chat_group_id", va: chatShow.chat_group_id || cookies.get("ck_cgi") },
							{ par: "user_id", va: store.getters.user.user_id },
							{ par: "organization_id", va: store.getters.user.organization_id },
						],
					}), SecretKey, cryoptojs
				).toString()
			},
			config
		)
		.then((response) => {
			let data = JSON.parse(response.data.data);
			if (data.length > 0) {
				if (showInfoChat.value != null) {
					//data[0][0].IsInfoChat = localStorage.getItem("ck_tabchat") != null ? localStorage.getItem("ck_tabchat") == 'true': showInfoChat.value;
					data[0][0].IsInfoChat = cookies.get("ck_tabchat") != null ? cookies.get("ck_tabchat") == 'true': showInfoChat.value;
				}
				detailChat.value = data[0][0];
				if (detailChat.value.is_group_chat != true) {
					if (store.getters.userConnected.findIndex(x => x.user_id == (detailChat.value.user_chat == store.getters.user.user_id ? detailChat.value.created_by : detailChat.value.user_chat) && x.connected) >= 0){
						detailChat.value.Online = true;
					}
					else {
						detailChat.value.Online = false;
					}
				}
				if (data[1].length > 0){
					data[1].forEach((el, idx) => {
						if (el.user_chat_name != null && el.user_chat_name.trim() != '') {
							el.user_chat_firstname = el.user_chat_name.trim().lastIndexOf(" ") >= 0 ? el.user_chat_name.trim().substring(el.user_chat_name.trim().lastIndexOf(" ") + 1) : el.user_chat_name.trim();
						}
						if (el.files != null){
							el.files = JSON.parse(el.files);
							if (el.files.length > 0) {
								el.files.forEach((fi) => {
									if (fi.type_save_file != null) {
										fi.type_save_file = parseInt(fi.type_save_file);
									}
								});
							}
						}						
						if (el.sticks != null){
							el.sticks = JSON.parse(el.sticks);
						}						
						if (el.seens != null){
							el.seens = JSON.parse(el.seens);
						}
						el.ParentComment = data[1].find(x => el.chat_parent_id === x.chat_message_id);
					});
				}
				listMessage.value = data[1];
				listMember.value = data[2];
				if (chatShow != null && chatShow.number_ms_unread > 0) {
					chatShow.number_ms_unread = 0;
				}
				//if (localStorage.getItem("ck_cgi") != detailChat.value.chat_group_id) {
				if (cookies.get("ck_cgi") != detailChat.value.chat_group_id) {
					forceRerender();
				}
				//localStorage.setItem("ck_cgi", detailChat.value.chat_group_id);
				cookies.set("ck_cgi", detailChat.value.chat_group_id);
			}
		})
		.catch((error) => {
			console.log("Tải dữ liệu không thành công! " + error);
			//toast.error("Tải dữ liệu không thành công!");

			if (error && error.status === 401) {
				swal.fire({
					
					text: "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại!",
					
					confirmButtonText: "OK",
				});
				store.commit("gologout");
			}
		});
};
const reloadDataChat = () => {
	//detailChat.value = null;
	axios
		.post(
			baseUrlCheck + "api/chat/GetDataProc",
			{ 
				str: encr(JSON.stringify({
						proc: "chat_group_detail",
						par: [
							//{ par: "chat_group_id", va: localStorage.getItem("ck_cgi") },
							{ par: "chat_group_id", va: cookies.get("ck_cgi") },
							{ par: "user_id", va: store.getters.user.user_id },
							{ par: "organization_id", va: store.getters.user.organization_id },
						],
					}), SecretKey, cryoptojs
				).toString()
			},
			config
		)
		.then((response) => {
			let data = JSON.parse(response.data.data);
			if (data.length > 0) {
				if (showInfoChat.value != null) {
					//data[0][0].IsInfoChat = localStorage.getItem("ck_tabchat") != null ? localStorage.getItem("ck_tabchat") == 'true': showInfoChat.value;
					data[0][0].IsInfoChat = cookies.get("ck_tabchat") != null ? cookies.get("ck_tabchat") == 'true': showInfoChat.value;
				}
				detailChat.value = data[0][0];
				//localStorage.setItem("ck_cgi", detailChat.value.chat_group_id);
				cookies.set("ck_cgi", detailChat.value.chat_group_id);
				if (data[1].length > 0){
					data[1].forEach((el, idx) => {
						if (el.user_chat_name != null && el.user_chat_name.trim() != '') {
							el.user_chat_firstname = el.user_chat_name.trim().lastIndexOf(" ") >= 0 ? el.user_chat_name.trim().substring(el.user_chat_name.trim().lastIndexOf(" ") + 1) : el.user_chat_name.trim();
						}						
						if (el.files != null){
							el.files = JSON.parse(el.files);
							if (el.files.length > 0) {
								el.files.forEach((fi) => {
									if (fi.type_save_file != null) {
										fi.type_save_file = parseInt(fi.type_save_file);
									}
								});
							}
						}												
						if (el.sticks != null){
							el.sticks = JSON.parse(el.sticks);
						}						
						if (el.seens != null){
							el.seens = JSON.parse(el.seens);
						}
						el.ParentComment = data[1].find(x => el.chat_parent_id === x.chat_message_id);
					});
				}
				listMessage.value = data[1];
				listMember.value = data[2];
				// emitter.emit("emitData", {
				// 	type: "renderMessage",
				// 	data:  null
				// });
			}
		})
		.catch((error) => {
			console.log(error);
			toast.error("Tải dữ liệu không thành công!");

			if (error && error.status === 401) {
				swal.fire({
					
					text: "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại!",
					
					confirmButtonText: "OK",
				});
				store.commit("gologout");
			}
		});
};
const isAddChat = ref();
const headerChat = ref();
const displayChat = ref(false);
const submitted = ref(false);
const typeGroupChat = ref();
const openAddGroupChat = (type, data) => {
	typeGroupChat.value = type;
	if (data != null && data.chat_group_id != null) {
		isAddChat.value = false;
		headerChat.value = "Thông tin cuộc trò chuyện";		
		loadDataListByID(data);
	}
	else {
		isAddChat.value = true;
		headerChat.value = "Thêm mới cuộc trò chuyện";
		chat.value = {
			chat_group_name: "",
			status: 1,
			organization_id: store.getters.user.organization_id,
			is_captain: true,
			created_by: store.getters.user.user_id,
			searchU: "",
			Focus: false,
		};
		listMember.value = [];
		listMessage.value = [];
	}
	displayChat.value = true;
	submitted.value = false;
};
const closeDialog = (type) => {
	displayChat.value = false;
	chat.value = {
		chat_group_name: "",
		status: 1,
		organization_id: store.getters.user.organization_id,
	};
};
const menuButs = ref();
const itemButs = ref([
	{
		label: "Tạo cuộc trò chuyện nhóm",
		icon: "pi pi-user-plus",
		command: (event) => {
			openAddGroupChat(1);
		},
	},
	{
		label: "Tạo cuộc trò chuyện cá nhân",
		icon: "pi pi-user-plus",
		command: (event) => {
			openAddGroupChat(0);
		},
	},
]);
const toggleAddChat = (event) => {
	menuButs.value.toggle(event);
};
const savingChat = ref(false);
const saveGroupChat = (isFormValid) => {
	submitted.value = true;
	if (savingChat.value == true) {
		return;
	}
	if (typeGroupChat.value == 0) {
		
	}
	else {
		if (!isFormValid) {
			return;
		}
		chat.value.is_group_chat = true;
	}
	var ms = isAddChat.value == true ? ({ chat_message_id: null }) : { chat_message_id: null }; 
	let formData = new FormData();
	for (var i = 0; i < files.length; i++) {
		let file = files[i];
		formData.append("avatarGroup", file);
	}
	if (chat.value.is_group_chat != true){
		listMember.value = [];
	}
	formData.append("models", JSON.stringify(chat.value));
	formData.append("members", JSON.stringify(listMember.value));
	formData.append("messages", JSON.stringify(ms));
	savingChat.value = true;
	swal.fire({
		width: 110,
		didOpen: () => {
		swal.showLoading();
		},
	});
	axios({
		method: isAddChat.value == false ? "put" : "post",
		url:
		//baseURL +
		baseUrlCheck + 
		`/api/chat/${isAddChat.value == false ? "Update_Chat" : "Add_Chat"}`,
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
			if (response.data.chatGroupID) {
				//localStorage.setItem("ck_cgi", response.data.chatGroupID);
				cookies.set("ck_cgi", response.data.chatGroupID);
			}
			loadDataGroupChat();
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
const chooseImage = (id) => {
  	document.getElementById(id).value = "";
  	document.getElementById(id).click();
};
//delete img
const delAvatar = () => {
	files = [];
	isDisplayAvt.value = false;
	var output = document.getElementById("groupChatAvt");
	output.src = basedomainURL + "/Portals/Image/image_group_user.jpg";
	chat.value.avatar_group = null;
};
const filterUser = ref([]);
const filterUserCopy = ref([]);
const filterListUser = (pb) => {
	var output = [];
	if (chat.value.searchU != null && chat.value.searchU.trim() != ""){
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
		if (listMember.value != null && listMember.value.length > 0) {
			listMember.value.forEach((item, idx) => {
				var i = output.findIndex(x => x.user_id === item.user_id || x.user_join === item.user_join);
				if (i !== -1) {
					output.splice(i, 1);
				}
			});
		}
		return output;
	}
	else {
		//filterUserCopy.value = [...filterUser.value];
		pb.listUserPB.filter(x => x.user_id !== store.getters.user.user_id).forEach((u, idx) => {
			u.user_join = u.user_id;
			u.is_order = idx;
			output.push(u);
		});
		return output;
	}
	// if ((chat.value.searchU == null || chat.value.searchU == "") && filterUser.value.length > 0){
	// 	return filterUser.value.filter(user => user.department_id == pb.department_id);
	// }
	// else if (chat.value.searchU != null && chat.value.searchU.trim() != "" && filterUserCopy.value.length > 0){
	// 	return filterUserCopy.value.filter(user => user.department_id == pb.department_id);
	// }
	// return [];
}
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
				el.listUserPB = filterUser.value.filter(x => x.department_id == el.department_id && x.user_id != store.getters.user.user_id && listMember.value.findIndex(y => y.user_join == x.user_id) < 0);
			});
			departments.value = data[0];
		})
		.catch((error) => {
			toast.error("Tải dữ liệu không thành công!");
			console.log("Error chat_departments_list_tree");
			if (error && error.status === 401) {
				swal.fire({
					
					text: "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại!",
					
					confirmButtonText: "OK",
				});
				store.commit("gologout");
			}
		});
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
		//$("input.ipautoUser" + i + ".true").focus();
		// var i = filterUser.value.findIndex(x => x.user_id === u.user_id);
		// if (i !== -1) {
		// 	filterUser.value.splice(i, 1);
		// }
		var iC = filterUserCopy.value.findIndex(x => x.user_id === u.user_id);
		if (iC !== -1) {
			filterUserCopy.value.splice(iC, 1);
		}
	}
	else {
		let listPersonalChat = datalists.value.filter(x => x.is_group_chat != true && ((x.user_chat == u.user_id && x.created_by == store.getters.user.user_id) || (x.created_by == u.user_id && x.user_chat == store.getters.user.user_id)));
		if (listPersonalChat.length == 0){
			chat.value.chat_group_name = u.full_name;
			chat.value.is_group_chat = false;
			chat.value.user_chat = u.user_id;
			chat.value.avatar_group = u.avatar;
			saveGroupChat();
		}
		else {
			showDetailChat(listPersonalChat[0]);
			displayChat.value = false;
		}
	}
};
const removeUser = (us, idx) => {
	if (us[idx].chat_member_id) {
		Out_GroupChat(chat, 3, us[idx].user_join, store.getters.user.user_id);
	}
	else {
		// if (filterUser.value.filter(x => x.user_id == us[idx].user_id).length == 0){
		// 	filterUser.value.push(us[idx]);
		// 	filterUser.value = filterUser.value.sort((a, b) => a.full_name - b.full_name);
		// }
		// else 
		if (filterUserCopy.value.filter(x => x.user_id == us[idx].user_id).length == 0){
			filterUserCopy.value.push(us[idx]);
			filterUserCopy.value = filterUserCopy.value.sort((a, b) => a.full_name - b.full_name);
		}
		us.splice(idx, 1);
	}
};
const Out_GroupChat = () => {

};
// Modal Tree User
const selectedUser = ref([]);
const is_one = ref(false);
const is_type = ref();
const headerDialogUser = ref();
const displayDialogUser = ref(false);
const closeDialogUser = () => {
  	displayDialogUser.value = false;
};
const showModalUser = (one, type) => {
	selectedUser.value = [];
	headerDialogUser.value = "Chọn người dùng";
	selectedUser.value = [...listMember.value];
	is_one.value = one;
	is_type.value = type;
	displayDialogUser.value = true;
};
const choiceUser = () => {
	switch (is_type.value) {
		case 0:
			var notexist = selectedUser.value.filter((a) => listMember.value.findIndex((b) => b["user_join"] === a["user_id"]) === -1);
			if (notexist.length > 0) {
				notexist.forEach((e) => {
					e.user_join = e.user_id;
				});
				listMember.value = listMember.value.concat(notexist);
			}
			break;
		default:
			break;
	}
	closeDialogUser();
};
//
const dataListSearch = () => {
	if (datalists.value.length > 0){
		if (options.value.SearchText != null && options.value.SearchText.trim() != '' && datalists.value.length > 0){
			//return datalists.value.filter(grChat => grChat.displayInList == 1 && grChat.chat_group_name.toLowerCase().includes(options.value.SearchText.toLowerCase()));
			return datalists.value.filter(grChat => grChat.displayInList == 1 && change_unsigned(grChat.chat_group_name).includes(change_unsigned(options.value.SearchText)));
		}
		return datalists.value.filter(grChat => grChat.displayInList == 1);
	}
	return datalists.value;
};
const nameUser_login = ref(store.getters.user);

const callSocket = (mes) => {
	socket.emit('sendData', mes);	
	callSendNotiChat(mes);
};
const SeenMess = (MessageID, User_ID) => {
	if (listMessage.value && listMessage.value.length > 0) {
		let newmess = listMessage.value.find(x => x.chat_message_id == MessageID);
		if (newmess && newmess.seens.filter(x => x.user_id == User_ID).length == 0) {
			let idxcm = listMessage.value.findIndex(x => x.chat_message_id == MessageID);
			listMessage.value.forEach(function (ms, k) {
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
		var data = response.data.data;
		//goBottomChat();
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
const callSendNotiChat = (item) => {
	if (item != null) {
		socketMethod
			.post("sendnotification", {
				uids: item["uids"],
				options: {
					title: "Trao đổi: " + item["title_chat"],
					text: item["type_message"] == 0 || item["type_message"] == 5 ? item["content_message"] 
							: item["type_message"] == 1 ? "[Hình ảnh]" : item["type_message"] == 2 ? "[File]" 
							: item["type_message"] == 3 ? "[Video]" : "[Audio]",
					image:
						baseURL +
						(store.getters.user.background_image ||
						"../assets/background/bg.png"),
					tag: "project.soe.vn",
					url: "chat/chat_message/".concat(item["chat_group_id"]),
				},
			})
			.then((res) => {});
	}
};
const initSocketDataMessage = (data, isGetRealtime) => {
	switch (data["event"]) {
		case "getSendMessage":
			SeenMess(data["chat_message_id"], data["sender"]);			
			loadDataGroupChat(isGetRealtime);
			break;
		case "getDelMessage":
			if (data["chat_group_id"] == detailChat.value.chat_group_id) {
				loadDataGroupChat();
			}
			break;
		case "OutUserChat":
			if (data["chat_group_id"] == detailChat.value.chat_group_id) {
				loadDataGroupChat();
			}
			break;
		default:
			break;
	}
};
const initSocketDataChat = (data) => {
	switch (data["event"]) {
		case "getDelChat":
			if (data["chat_group_id"] == props.detailChat.chat_group_id) {
				loadDataGroupChat();
			}
			break;
		default:
			break;
	}
};
onMounted(() => {
	if (!checkURL(window.location.pathname, store.getters.listModule)) {
		//router.back();
	}
	loadDataGroupChat();
	listDepartmentsUser();
	socket.on("getSendMessage", (data) => {
		var users = store.getters.userConnected;
		for (let i = 0; i < users.length; i++) {
			const user = users[i];
			if (user.connected && user.user_id === data.user_id) {
				initSocketDataMessage(data, true);
			}
		}
	});
	
	socket.on("getDelMessage", (data) => {
		var users = store.getters.userConnected;
		for (let i = 0; i < users.length; i++) {
			const user = users[i];
			if (user.connected && user.user_id === data.user_id) {
				initSocketDataMessage(data);
			}
		}
	});
	
	socket.on("getDelChat", (data) => {
		var users = store.getters.userConnected;
		for (let i = 0; i < users.length; i++) {
			const user = users[i];
			if (user.connected && user.user_id === data.user_id) {
				initSocketDataChat(data);
			}
		}
	});

	socket.on("OutUserChat", (data) => {
		var users = store.getters.userConnected;
		for (let i = 0; i < users.length; i++) {
			const user = users[i];
			if (user.connected && user.user_id === data.user_id) {
				initSocketDataMessage(data);
			}
		}
	});
	socket.on("reloadListChat", (data) => {
		var users = store.getters.userConnected;
		for (let i = 0; i < users.length; i++) {
			const user = users[i];
			if (user.connected && user.user_id === data.user_id) {
				for (let i = 0; i < datalists.value.length; i++) {
					if (datalists.value.is_group_chat != true && datalists.value.displayInList == 1) {				
						let userChat = datalists.value[i].user_chat == store.getters.user.user_id ? datalists.value[i].created_by : datalists.value[i].user_chat;
						if (users.findIndex(x => x.user_id == userChat && x.connected) >= 0) {
							datalists.value[i].Online = true;
						}
						else{
							datalists.value[i].Online = false;
						}
					}
					else {
						datalists.value[i].Online = false;
					}
				}
			}
		}
	});
	return {
		options,
		loadDataGroupChat,
		listDepartmentsUser,
	};
});
</script>
<template>
	<div class="law-wrap">
		<div class="law-header" style="padding-top: 0.5rem;"></div>
		<div class="law-body" style="height:calc(100vh - 4rem); margin: 0;">
			<Splitter class="w-full" style="height:calc(100vh - 4rem);border-top: none;">
				<SplitterPanel :size="25" style="min-width:20%;">
					<div class="d-lang-table">
						<div class="col-12 flex" style="border-bottom: 1px solid #dee2e6;background-color: #ffffff;color: #000000;padding: 0.9rem !important;">
							<span class="p-input-icon-left w-full flex search-filter">
								<i class="pi pi-search" />
								<InputText type="text" spellcheck="false" v-model="options.SearchText" placeholder="Tìm kiếm cuộc trò chuyện"
									class="inputtext-filter"
								/>
								<Button class="p-button-sm p-button-outlined p-button-secondary bg-white ml-2 px-3 py-2 btn-add-chat"
									type="button" icon="pi pi-users" 
									@click="toggleAddChat"
									aria-haspopup="true"
									aria-controls="overlay_addChat"
								/>
								<Menu
									id="overlay_addChat"
									ref="menuButs"
									:model="itemButs"
									:popup="true"
									style="width:fit-content;"
								/>
							</span>
						</div>
						<DataView 
							class="w-full h-full ptable p-datatable-sm flex flex-column table-list-groupchat pl-2"
							:lazy="true"
							:value="dataListSearch()" 
							:layout="layout" 
							:loading="options.loading"
							:rowHover="true"
							responsiveLayout="scroll"
							:scrollable="true"
							style="height: calc(100vh - 8.5rem) !important;"
						>
							<template #list="slotProps">
								<div class="grid p-2 m-0 w-full dataListGroupChat" 
									:class="chat_group_active == slotProps.data.chat_group_id ? 'row-active-groupchat' : ''"
									@click="showDetailChat(slotProps.data)"
									style="background-color: #fff;">
									<div class="w-3rem pt-2 flex" style="justify-content:center;line-height:1.5;position: relative;height: 58px;">
										<img class="ava" width="48" height="48" 
											v-bind:src="slotProps.data.avatar_group
														? basedomainURL + slotProps.data.avatar_group
														: basedomainURL + '/Portals/Image/image_group_user.jpg'
													"
											@error="$event.target.src = basedomainURL + (slotProps.data.is_group_chat ? '/Portals/Image/image_group_user.jpg' : '/Portals/Image/nouser1.png')"
											v-if="slotProps.data.avatar_group || slotProps.data.is_group_chat == true"
											style="border:1px solid #ccc;"
										/>
										<Avatar v-else
											class="avt-replace"
											size="large"
											shape="circle"
											v-bind:label="(slotProps.data.chat_group_lastname ?? '').substring(0, 1)"
											style="cursor: pointer;width: 48px; height: 48px;border:1px solid #ccc;"
											:style="{ background: (slotProps.data.is_group_chat ? '' : bgColor[slotProps.data.is_order % 7]) + '!important'}"
										/>
										<span class="online" style="bottom:3px;right:-4px;" v-if="slotProps.data.Online"></span>
										<span class="offline" style="bottom:3px;right:-4px;" v-if="(!slotProps.data.Online && !slotProps.data.is_group_chat)"></span>
									</div>
									<div class="flex pl-2 py-0" style="width:calc(100% - 3rem);flex-direction: column;justify-content: center;">
										<div class="col-12 pb-1 flex">
											<div class="flex-1" 
												style="width: calc(100% - 4rem); display: inline-grid; word-break: break-word; background: transparent;"
												:style="slotProps.data.number_ms_unread > 0 ? 'color:#0078d4;' : ''"
											>
												<span class="font-bold chatgroup-name">
													{{slotProps.data.chat_group_name}}
												</span>
											</div>
											<div class="info-law w-2rem" style="display: flex; justify-content: end; align-items: center;">
												<Badge :value="slotProps.data.number_ms_unread" severity="danger" v-if="slotProps.data.number_ms_unread > 0"></Badge>
											</div>
											<div class="info-law w-2rem" style="display: flex; justify-content: end; align-items: center; color: #f59300; font-size: 1.15rem;" v-if="slotProps.data.is_notify != true">
												<font-awesome-icon icon="fa-solid fa-bell-slash" />
											</div>
										</div>
										<div class="col-12 pt-1 pr-0 flex" v-if="slotProps.data.user_last_chat != null || slotProps.data.last_message || slotProps.data.last_message_time">
											<div class="info-law flex-1" style="width: calc(100% - 6rem); display: inline-grid; word-break: break-word; background: transparent;">
												<span class="mr-2" style="white-space: nowrap; overflow: hidden; text-overflow: ellipsis;">
													<span class="font-medium" :style="slotProps.data.number_ms_unread > 0 ? 'color:#0078d4;' : ''">
														{{ slotProps.data.user_last_chat != null ? (slotProps.data.user_last_chat + ": ") : '' }}
													</span>
													<span :style="slotProps.data.number_ms_unread > 0 ? 'font-weight:bold;color:#0078d4;' : ''"
														v-html="slotProps.data.last_message 
																? (slotProps.data.last_message.includes('<br/>') ? slotProps.data.last_message.substring(0, slotProps.data.last_message.indexOf('<br/>')) : slotProps.data.last_message) 
																: ''"
													>
													</span>													
												</span>
											</div>
											<div class="info-law w-6rem" style="justify-content:center;">
												<div class="btn-download-law" style="text-align: right;">
													<span class="" style="font-size:13px;">
														{{ slotProps.data.last_message_time 
															? (Math.floor(Math.abs(new Date(slotProps.data.date_now) - new Date(slotProps.data.last_message_time)) / 3600000) >= 24
																? moment(new Date(slotProps.data.last_message_time)).format("HH:mm DD/MM") 
																: (Math.floor(Math.abs(new Date(slotProps.data.date_now) - new Date(slotProps.data.last_message_time)) / 3600000) < 1
																	? (Math.floor(Math.abs(new Date(slotProps.data.date_now) - new Date(slotProps.data.last_message_time)) / 60000) > 1
																		? (Math.floor(Math.abs(new Date(slotProps.data.date_now) - new Date(slotProps.data.last_message_time)) / 60000) + ' phút trước')
																		: 'vừa xong')
																	: (Math.floor(Math.abs(new Date(slotProps.data.date_now) - new Date(slotProps.data.last_message_time)) / 3600000) + ' giờ trước')
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
							</template>
							<template #empty>
								<div class="align-items-center justify-content-center p-4 text-center"
									v-if="datalists.length == 0"
									>
									<img src="../../assets/background/nodata.png" height="144" />
									<h3 class="m-1">Không có dữ liệu</h3>
								</div>
							</template>
						</DataView>
					</div>
				</SplitterPanel>
				<SplitterPanel :size="75" style="min-width:70%;">
					<!-- <div class="comp-chat" v-if="detailChat != null && detailChat.chat_group_id != null"> -->
					<div class="comp-chat" v-if="detailChat != null">
						<chatMessage
							:key="componentKey"
							:detailChat="detailChat"
							:listMessage="listMessage"
							:listMember="listMember"
							:listGroupChat="datalists"
							:refreshData="reloadDataChat"
							:funcCallSocket="callSocket"
							:funcCallUpdate="loadListChatGroup"
						/>
					</div>	
					<div id="nodata" v-else>
						<div style="display: table; height: calc(100vh - 50px); width: 100%">
							<div style="text-align:center;display: table-cell; vertical-align: middle;margin: auto;width: 100%;">
								<img width="100" v-bind:src="basedomainURL + '/Portals/Module/TRAODOI.png'" />
								<div class="flex" style="align-items:center; justify-content:center;">
									<div class="mr-4" style="position:relative;">
										<img class="ava" width="65" height="65" 
											v-bind:src="nameUser_login.avatar
														? basedomainURL + nameUser_login.avatar
														: basedomainURL + '/Portals/Image/noimg.jpg'
													"
											@error="$event.target.src = basedomainURL + '/Portals/Image/nouser1.png'"
											v-if="nameUser_login.avatar"
										/>
										<Avatar v-else
											class="avt-replace"
											shape="circle"
											v-bind:label="(nameUser_login.last_name ?? '').substring(0, 1)"
											style="cursor: pointer;width: 65px; height: 65px;"
											:style="{ background: bgColor[0] + '!important' }"
										/>
										<span class="online" style="width: 1.25em;height: 1.25rem;"></span>
									</div>
									<p style="color: #3a3a3a; font-size: 2rem; line-height: 1.5;">Chào mừng bạn <br/><b>{{nameUser_login.full_name || ''}}</b></p>
								</div>				
								<div class="flex" style="height:15rem;"></div>
							</div>
						</div>
					</div>				
				</SplitterPanel>
			</Splitter>
		</div>
	</div>
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
						<!-- <div class="inputanh relative flex" style="margin: 0 auto;width:100%; height:auto;"> -->
						<div class="inputanh relative flex" style="margin: 0 auto;">
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
								<ul class="my-0" style="float: left; padding: 0; width: 100%;" v-if="listMember.length > 0">
									<li style="width: 100%; margin: 5px 0; float: left; list-style-type: none " 
										v-for="(m, index) in listMember" :key="index"
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
												@click="removeUser(listMember,index)">
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
									style="cursor:pointer;color:rgb(33, 150, 243);"
									@click="showModalUser(false,0)">
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
												@click="clickAddUser(chat,listMember,u,'')">
												<div class="" style="display: -webkit-box; display: -webkit-inline-box; justify-content: flex-start; width: 100%;">
													<img class="ava" width="48" height="48" 
														v-bind:src="u.avatar
																	? basedomainURL + u.avatar
																	: basedomainURL + '/Portals/Image/noimg.jpg'
																"
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
	<!-- Tree user -->
	<treeuser
		v-if="displayDialogUser === true"
		:headerDialog="headerDialogUser"
		:displayDialog="displayDialogUser"
		:closeDialog="closeDialogUser"
		:one="is_one"
		:selected="selectedUser"
		:choiceUser="choiceUser"
	/>
</template>

<style scoped>
	.inputanh {
		width: 100px;
		height: 100px;
		cursor: pointer;
		border-radius: 50%;
    	border: 1px solid #ced4da;
	}
	.inputanh img {
		object-fit: cover;
		width: 100%;
		height: 100%;
		border-radius: 50%;
    	border: 3px solid #fff;
	}
	.ip-us {
		padding: 0.305rem 0.75rem;
		border: 1px solid #ced4da;
		border-radius: 0.25rem;
		background-color: #fff;
	}
	.list-group {
		display: flex;
		-webkit-box-orient: vertical;
    	-webkit-box-direction: normal;
		flex-direction: column;
		padding-left: 0;
		margin-bottom: 0;
		margin-top: 0;
	}
	img.ava {
		border-radius: 50%;
		vertical-align: middle;
		object-fit: cover;
		border: 1px solid #e7e7e7;
	}
	.format_center {
		display: flex;
		justify-content: center;
		vertical-align: middle;
		align-items: center;
	}
	.format_text {
		white-space: nowrap;
		overflow: hidden;
		text-overflow: ellipsis;
		width: calc(100% - 50px) !important;
	}
	li.list-group-item:hover {
		cursor: pointer;
		background-color: aliceblue;
	}
	div.label-chat {
		padding: 2px 6px;
		border-radius: 30px;
		margin-right: 5px;
		/* margin-top: 5px; */
	}
	.btn-info-user-chat {
		color: #fff;
		background-color: #00bcd4;
		border-color: #00bcd4;
	}
	.law-wrap {
		height: 100%;
	}
	.law-body {
  		height: 100%;
	}
	.row-active-groupchat {
		background-color: #f0f8ff !important;
	}
	.dataListGroupChat:hover .chatgroup-name {
  		cursor: pointer;
	}
	.chatgroup-name {
		line-height: 1.5;
	}
	.online {
		position: absolute;
		width: 1rem;
		height: 1rem;
		background-color: #0ad932;
		border-radius: 50%;
		bottom: 0;
		right: 0;
		border: solid 1px #fff;
	}	
	.offline {
		position: absolute;
		width: 1rem;
		height: 1rem;
		background-color: #ff9900;
		border-radius: 50%;
		bottom: 0;
		right: 0;
		border: solid 1px #fff;
	}
</style>
<style lang="scss" scoped>
	::v-deep(.btn-add-chat) {
		.p-button-icon {
			font-size: 1.25rem;
		}
	}
	::v-deep(.table-list-groupchat) {
		div.p-col.col {
			border-bottom: none
			;
		}
		.p-dataview-content>.p-grid>div.p-col.col:hover {
			background-color: transparent !important;
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