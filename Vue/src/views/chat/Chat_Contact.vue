<script setup>
import { ref, inject, onMounted, watch } from "vue";
import { useToast } from "vue-toastification";
import { required, maxLength } from "@vuelidate/validators";
import { useVuelidate } from "@vuelidate/core";
import { VuemojiPicker } from "vuemoji-picker";
import moment from "moment";
import { useCookies, globalCookiesConfig } from "vue3-cookies";
import chatMessage from "../../components/chat/DetailChat.vue";
import { encr, change_unsigned, checkURL } from "../../util/function.js";
import { socketMethod } from "../../util/methodSocket";
const cryoptojs = inject("cryptojs");
//Khai báo
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
if (baseURL.includes("soe.vn")) {
  globalCookiesConfig({
    expireTimes: "30d",
    path: "/",
    domain: ".soe.vn",
  });
}
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
const config = {
  	headers: { Authorization: `Bearer ${store.getters.token}` },
};

const bgColor = ref([
    "#F8E69A", "#AFDFCF", "#F4B2A3", "#9A97EC", "#CAE2B0", "#8BCFFB", "#CCADD7"
]);

const options = ref({
	sort: "full_name",
	SearchText: "",
	loading: true,
	TypeChat: 0,
	organization_id: store.getters.user.organization_id,
	user_key: store.getters.user.user_key,
});
const basedomainURL = baseURL;
const baseUrlCheck = baseURL;
const toast = useToast();
const layout = ref("list");
const chat = ref({
	chat_group_name: "",
	status: 1,
	organization_id: store.getters.user.organization_id,
});
//Thêm log
const addLog = (log) => {
  	axios.post(baseURL + "/api/Proc/AddLog", log, config);
};
const datalists = ref([]);
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
		data[0].forEach((el, idx) => {
			el.is_order = idx;
		});
		datalists.value = data[0];
		options.value.loading = false;
		// if (localStorage.getItem("ck_cgi") != null){
		// 	reloadDataChat(null, isGetRealtime);
		// }
		if (cookies.get("ck_cgi") != null){
			reloadDataChat(null, isGetRealtime);
		}
		else {
			detailChat.value = null;
		}
		if ((data[1] == null || (data[1] != null && data[1][0] != null && data[1][0].is_notify)) && isGetRealtime) {
			let audioNoti = new Audio(baseUrlCheck + "/Portals/FileChatSystem/pristine-sound.mp3");
			audioNoti.play();
		}
      })
      .catch((error) => {
        toast.error("Tải dữ liệu không thành công!");
        options.value.loading = false;
        addLog({
			title: "Lỗi Console loadData",
			controller: "SQLView.vue",
			logcontent: error.message,
			loai: 2,
        });
        if (error && error.status === 401) {
			swal.fire({
				text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
				confirmButtonText: "OK",
			});
			store.commit("gologout");
        }
      });
};

const detailChat = ref();
const listMessage = ref ([]);
const listMember = ref ([]);
const reloadDataChat = (dataChat, isGetRealtime) => {
	//detailChat.value = null;
	axios
		.post(
			baseUrlCheck + "api/chat/GetDataProc",
			{ 
				str: encr(JSON.stringify({
						proc: "chat_group_detail",
						par: [
							//{ par: "chat_group_id", va: dataChat != null ? dataChat.chat_group_id : localStorage.getItem("ck_cgi") },
							{ par: "chat_group_id", va: dataChat != null ? dataChat.chat_group_id : cookies.get("ck_cgi") },
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
				if (dataChat != null && dataChat.number_ms_unread > 0) {
					dataChat.number_ms_unread = 0;
				}
				if (cookies.get("ck_cgi") != detailChat.value.chat_group_id) {
					forceRerender();
				}
				//localStorage.setItem("ck_cgi", detailChat.value.chat_group_id);
				cookies.set("ck_cgi", detailChat.value.chat_group_id);
			}
		})
		.catch((error) => {
			//console.log(error);
			toast.error("Tải dữ liệu không thành công!");

			if (error && error.status === 401) {
				swal.fire({
					
					text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
					
					confirmButtonText: "OK",
				});
				store.commit("gologout");
			}
		});
};

const filterUser = ref([]);
const filterUserCopy = ref([]);
const filterListUser = (pb, typeList) => {
	if (typeList == 'phongban'){
		if (options.value.SearchText != null && options.value.SearchText.trim() != ""){
			var output = [];
			filterUser.value.filter(x => x.user_id !== store.getters.user.user_id).forEach((u, idx) => {
				//if ((u.full_name || "").toLowerCase().indexOf(options.value.SearchText.toLowerCase()) >= 0) {
				let keySearchUnsign = change_unsigned(options.value.SearchText);
				if (change_unsigned(u.full_name || "").indexOf(keySearchUnsign) >= 0 || u.user_id.indexOf(keySearchUnsign) >= 0) {
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
					var i = output.findIndex(x => x.user_id === item.user_id);
					if (i !== -1) {
						output.splice(i, 1);
					}
				});
			}
			filterUserCopy.value = output;
		}
		else {
			filterUserCopy.value = [...filterUser.value];
		}
		if ((options.value.SearchText == null || options.value.SearchText == "") && filterUser.value.length > 0){
			return filterUser.value.filter(user => user.department_id == pb.department_id);
		}
		else if (options.value.SearchText != null && options.value.SearchText.trim() != "" && filterUserCopy.value.length > 0){
			return filterUserCopy.value.filter(user => user.department_id == pb.department_id);
		}
	}
	else {
		if (options.value.SearchText != null && options.value.SearchText.trim() != "" && filterUser.value.length > 0){
			//return filterUser.value.filter(user => user.full_name.toLowerCase().includes(options.value.SearchText.toLowerCase()));
			let keySearchUnsign = change_unsigned(options.value.SearchText);
			return filterUser.value.filter(user => change_unsigned(user.full_name).includes(keySearchUnsign) || user.user_id.includes(keySearchUnsign));
		}
		return filterUser.value;
	}
	return [];
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
							{ par: "user_id", va: store.getters.user.user_id },
						],
					}), SecretKey, cryoptojs
				).toString()
			},
			config
		)
		.then((response) => {
			let data = JSON.parse(response.data.data);
			departments.value = data[0];
			var users = store.getters.userConnected;
			for (let i = 0; i < data[1].length; i++) {
				let userChat = data[1][i].user_id;
				if (users.findIndex(x => x.user_id == userChat && x.connected) >= 0) {
					data[1][i].Online = true;
				}
				else{
					data[1][i].Online = false;
				}
			}
			filterUser.value = data[1];
			filterUserCopy.value = data[1];
		})
		.catch((error) => {
			toast.error("Tải dữ liệu không thành công!");
			console.log("Error chat_departments_list_tree");
			if (error && error.status === 401) {
				swal.fire({
					
					text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
					
					confirmButtonText: "OK",
				});
				store.commit("gologout");
			}
		});
};
const selectTypeList = ref('all');
const typesListContact = ref([
	{ name: 'Tất cả người dùng', code: 'all' },
	{ name: 'Theo phòng ban', code: 'phongban' },
]);

const clickAddUser = (u) => {
	let listPersonalChat = datalists.value.filter(x => x.is_group_chat != true && ((x.user_chat == u.user_id && x.created_by == store.getters.user.user_id) || (x.created_by == u.user_id && x.user_chat == store.getters.user.user_id)));
	if (listPersonalChat.length == 0){
		chat.value.chat_group_name = u.full_name;
		chat.value.is_group_chat = false;
		chat.value.user_chat = u.user_id;
		chat.value.avatar_group = u.avatar;
		saveGroupChat();
	}
	else {
		reloadDataChat(listPersonalChat[0]);
	}
};
const isAddChat = ref(true);
const savingChat = ref(false);
const saveGroupChat = (isFormValid) => {	
	if (savingChat.value == true) {
		return;
	}
	var ms = isAddChat.value == true ? ({ chat_message_id: null }) : { chat_message_id: null }; 
	let formData = new FormData();
	// for (var i = 0; i < files.length; i++) {
	// 	let file = files[i];
	// 	formData.append("avatarGroup", file);
	// }
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
			//localStorage.setItem("ck_cgi", response.data.chatGroupID);
			cookies.set("ck_cgi", response.data.chatGroupID);
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
	listDepartmentsUser();
	loadDataGroupChat();
	
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
		selectTypeList,
		listDepartmentsUser,
		loadDataGroupChat,
	};
});
</script>
<template>
	<!-- <div class="surface-100 bg-white law-wrap"> -->
	<div class="law-wrap">
		<div class="law-header" style="padding-top: 0.5rem;"></div>
		<div class="law-body" style="height:calc(100vh - 3.75rem); margin: 0;">
			<Splitter class="w-full" style="height:calc(100vh - 3.75rem)">
				<SplitterPanel :size="25" style="min-width:20%;">
					<div class="d-lang-table">
						<div class="col-12 flex" style="background-color: #ffffff;color: #000000;padding: 0.5rem !important;">
							<span class="p-input-icon-left w-full flex search-filter">
								<i class="pi pi-search" />
								<InputText type="text" spellcheck="false" 
									v-model="options.SearchText" placeholder="Tìm kiếm ..."
									class="inputtext-filter"
								/>
							</span>
						</div>
						<div class="col-12 pt-0 flex">
							<Dropdown v-model="selectTypeList" 
								:options="typesListContact" 
								optionLabel="name" optionValue="code"
							/>
						</div>
						<DataView 
							class="w-full h-full ptable p-datatable-sm flex flex-column table-list-groupchat pl-2"
							:lazy="true"
							:value="filterListUser(null, selectTypeList)" 
							:layout="layout" 
							:loading="options.loading"
							:rowHover="true"
							responsiveLayout="scroll"
							:scrollable="true"
							style="height: calc(100vh - 11.25rem) !important;"
							v-if="selectTypeList == 'all'"
						>
							<template #list="slotProps">
								<div class="grid p-2 m-0 w-full dataListGroupChat" 
									:class="chat_group_active == slotProps.data.chat_group_id ? 'row-active-groupchat' : ''"
									@click="clickAddUser(slotProps.data)"
									style="background-color: #fff;">
									<div class="w-3rem pt-2 flex" style="justify-content:center;line-height:1.5;position: relative;height: 58px;">
										<img class="ava" width="48" height="48" 
											v-bind:src="slotProps.data.avatar
														? basedomainURL + slotProps.data.avatar
														: basedomainURL + '/Portals/Image/noimg.jpg'
													"
											@error="$event.target.src = basedomainURL + '/Portals/Image/nouser1.png'"
											v-if="slotProps.data.avatar"
											style="border:1px solid #ccc;"
										/>
										<Avatar v-else
											class="avt-replace"
											size="large"
											shape="circle"
											v-bind:label="(slotProps.data.last_name ?? '').substring(0, 1)"
											style="cursor: pointer;width: 48px; height: 48px;border:1px solid #ccc;"
											:style="{ background: bgColor[slotProps.data.is_order % 7] + '!important'}"
										/>
										<span class="online" style="bottom:3px;right:-4px;" v-if="slotProps.data.Online"></span>
									</div>
									<div class="flex pl-2 py-0" style="width:calc(100% - 3rem);flex-direction: column;justify-content: center;">
										<div class="col-12 pb-1 flex">
											<span class="font-bold chatgroup-name">
												{{ slotProps.data.full_name != null ? slotProps.data.full_name : '' }}
											</span>
										</div>
										<div class="col-12 pt-1 pr-0 flex" v-if="slotProps.data.user_id != null || slotProps.data.position_name">
											<div class="info-law flex-1" style="width: 100%; display: inline-grid; word-break: break-word; background: transparent;">
												<span class="mr-2" style="white-space: nowrap; overflow: hidden; text-overflow: ellipsis;">
													<span class="font-medium">
														{{ slotProps.data.user_id != null ? slotProps.data.user_id : '' }}
													</span>
													{{ slotProps.data.position_name ? (' | ' + slotProps.data.position_name) : '' }}
												</span>
											</div>
										</div>
									</div>
								</div>
							</template>
							<template #empty>
								<div class="align-items-center justify-content-center p-4 text-center"
									v-if="(filterListUser(null, selectTypeList)).length == 0"
									>
									<img src="../../assets/background/nodata.png" height="144" />
									<h3 class="m-1">Không có dữ liệu</h3>
								</div>
							</template>
						</DataView>
						<div class="flex" v-else>
							<div class="scrollParenttree w-full" style="overflow-x: hidden; overflow-y: auto;">
								<ul class="list-group dataUser p-0 pl-2 m-0" tabindex="0" style="max-height: calc(100vh - 11.25rem);">
									<div v-for="pb in departments" :key="pb.department_id" class="p-0">
										<div class="p-3" style="background-color: antiquewhite;">
											<span class="font-bold uppercase" style="color: #333;">{{pb.department_name}}</span>
										</div>											
										<li class="list-group-item format_center"
											style="padding:0.5rem 0.25rem;" 
											v-for="(u, index) in filterListUser(pb, 'phongban')" :key="index"
											@click="clickAddUser(u)">
											<div class="" style="display: -webkit-box; display: -webkit-inline-box; justify-content: flex-start; width: 100%;">
												<div style="position: relative;">
													<img class="ava" width="48" height="48" 
														v-bind:src="u.avatar
																	? basedomainURL + u.avatar
																	: basedomainURL + '/Portals/Image/noimg.jpg'
																"
														v-if="u.avatar"
														style="border:1px solid #ccc;"
													/>
													<Avatar v-else
														class="avt-replace"
														size="large"
														shape="circle"
														v-bind:label="(u.last_name ?? '').substring(0, 1)"
														style="cursor: pointer;width: 48px; height: 48px;border: 1px solid #ccc;"
														:style="{ background: bgColor[u.is_order % 7] + '!important'}"
													/>
													<span class="online" style="bottom:10px;right:0px;" v-if="u.Online"></span>
												</div>
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
</template>

<style scoped>
	img.ava {
		border-radius: 50%;
		vertical-align: middle;
		object-fit: cover;
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