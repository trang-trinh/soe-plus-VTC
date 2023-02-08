<script setup>
import { ref, inject, onMounted, watch } from "vue";
import { useToast } from "vue-toastification";
import { useVuelidate } from "@vuelidate/core";
import dialogScreen from "../../components/tivi/tivi_screen_detail.vue";
import dialogDataScreen from "../../components/tivi/tivi_video_list.vue";
import dialogDataImage from "../../components/tivi/tivi_image_list.vue";
import dialogDataDBScreen from "../../components/tivi/tivi_screen_list.vue";
import { change_unsigned, getParent, encr, decr, checkURL } from "../../util/function.js";

const cryoptojs = inject("cryptojs");
const axios = inject("axios");
const store = inject("store");
const swal = inject("$swal");
const socket = inject("socket");
const router = inject("router");
const toast = useToast();
const basedomainURL = baseURL;
const baseUrlCheck = baseURL;
const config = {
	headers: { Authorization: `Bearer ${store.getters.token}` },
};

const option = ref({
	loading: false,
});
const screenTivi = ref();
//const v$ = useVuelidate(rules, screenTivi);
const listDataTivi = ref([]);
const listTivi = () => {
	axios
		.post(
			baseUrlCheck + "api/Tivi/GetDataProc",
			{ 
				str: encr(JSON.stringify({
						proc: "tivi_screen_list",
						par: [
							{ par: "user_id", va: store.getters.user.user_id },
						],
					}), SecretKey, cryoptojs
				).toString()
			},
			config
    ).then((response) => {
		var data = JSON.parse(response.data.data);
		listDataTivi.value = data[0];
	})
    .catch((error) => {
		if (error.status === 401) {
          swal.fire({
            text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
            confirmButtonText: "OK",
          });
          store.commit("gologout");
        }
		else {
			swal.fire({
				title: "Error!",
				text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
				icon: "error",
				confirmButtonText: "OK",
			});
		}
    });
};

const searchTivi = ref('');
const enterSearchTivi = ref('');
const searchScreenTV = () => {
	if (searchTivi.value != null && searchTivi.value.trim() != "") {
		enterSearchTivi.value = searchTivi.value.trim();
	}
	else {
		enterSearchTivi.value = "";
	}
};
const filterListTivi = () => {	
	if (enterSearchTivi.value != null && enterSearchTivi.value != "") {
		let keySearch = change_unsigned(enterSearchTivi.value);
		return listDataTivi.value.filter(tv => change_unsigned(tv.tivi_name).includes(keySearch));
	}
	else {
		return listDataTivi.value;
	}
};
const saveConfigTV = ref(false);
const saveConfigTivi = () => {
	if (saveConfigTV.value) {
		return;
	}
  	swal.fire({
		width: 110,
		didOpen: () => {
			swal.showLoading();
		},
	});
	let formData = new FormData();
	formData.append("modelTivi", JSON.stringify(detailTivi.value));
	formData.append("listScreen", JSON.stringify(listDataScreen.value));
	saveConfigTV.value = true;
	axios
		.post(baseUrlCheck + "/api/Tivi/Update_Tivi", formData, config)
		.then((response) => {
			if (response.data.err === "1" || response.data.err === "2") {
				swal.fire({
					title: "Thông báo!",
					text: response.data.ms,
					icon: response.data.err == 1 ? "error" : "warning",
					confirmButtonText: "OK",
				});
				return;
			}
			swal.close();
			if (saveConfigTV.value) saveConfigTV.value = false;
			toast.success("Cập nhật cấu hình tivi thành công!");
			selectRowTivi();
		})
		.catch((error) => {
			swal.close();
			if (saveConfigTV.value) saveConfigTV.value = false;
			swal.fire({
				title: "Thông báo!",
				text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
				icon: "error",
				confirmButtonText: "OK",
			});
			return;
		});
};
const delSingleTivi = (dataTivi) => {
	swal
		.fire({
			title: "Thông báo",
			text: "Bạn có muốn xoá tivi này không!",
			icon: "warning",
			showCancelButton: true,
			confirmButtonColor: "#3085d6",
			cancelButtonColor: "#d33",
			confirmButtonText: "Có",
			cancelButtonText: "Không",
		})
		.then((result) => {
		if (result.isConfirmed) {
			swal.fire({
				width: 110,
				didOpen: () => {
					swal.showLoading();
				},
			});

			axios
				.delete(baseUrlCheck + "/api/Tivi/Delete_Tivi", {
					headers: { Authorization: `Bearer ${store.getters.token}` },
					data: dataTivi != null ? [dataTivi.tivi_id] : "-1",
			})
			.then((response) => {
				swal.close();
				if (response.data.err != "1") {
					toast.success("Xoá tivi thành công!");
					listTivi();
					id_tivi_active.value = null;
					listDataScreen.value = [];
					id_screen_active.value = null;
					detailScreen.value = null;
				} else {
					swal.fire({
						title: "Thông báo",
						text: "Xảy ra lỗi khi xóa tivi",
						icon: "error",
						confirmButtonText: "OK",
					});
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
// const delMultiTivi = () => {
// 	// del tivi
// };
const reloadTivi = (data) => {
	var contentReload = { tivi_id: data.tivi_id, user_id: "chatbot" };
	socket.emit("sendData", { event: "reloadDataTivi", contentReload });
	toast.success("Gửi lệnh reload tivi thành công.");

	// axios
    // .post(
	//   basedomainURL + "/api/Tivi/List_TiviPublic",
    //   {
	// 	str: encr(JSON.stringify({ }), SecretKey, cryoptojs).toString()
    //   },
    //   config
    // )
    // .then((response) => {
    //   if (response != null && response.data != null) {
    //     var data = response.data.data;
    //     if (data != null) {
    //       	var dataTivi = JSON.parse(decr(data, SecretKey, cryoptojs));
	// 	  	debugger;
	// 		socket.emit("listTivi", dataTivi[0]);
	// 		toast.success("Gửi lệnh thành công.");
    //     }
    //   }
    // })
    // .catch((error) => {});
};
const onCheckBox = (value) => {
	let data = {
		TextID: value.tivi_id,
		BitTrangthai: value.is_active,
	};
	axios
		.put(baseUrlCheck + "/api/Tivi/Update_StatusTivi", data, config)
		.then((response) => {
			if (response.data.err != "1") {
				toast.success("Cập nhật trạng thái thành công!");
				listTivi();
			} 
			else {
				swal.fire({
					title: "Thông báo",
					text: response.data.ms,
					icon: "error",
					confirmButtonText: "OK",
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
};
const listDataScreen = ref([]);
const detailTivi = ref();
const id_tivi_active = ref();
const selectRowTivi = (event) => {
	listDataScreen.value = [];
	if (event != null) {
		if (id_tivi_active.value != event.data.tivi_id) {
			id_screen_active.value = null;
		}
		id_tivi_active.value = event.data.tivi_id;
	}
	axios
		.post(
			baseUrlCheck + "api/Tivi/GetDataProc",
			{ 
				str: encr(JSON.stringify({
						proc: "tivi_screen_detail_list",
						par: [
							{ par: "user_id", va: store.getters.user.user_id },
							{ par: "tivi_id", va: event != null ? event.data.tivi_id : id_tivi_active.value },
						],
					}), SecretKey, cryoptojs
				).toString()
			},
			config
    ).then((response) => {
		var data = JSON.parse(response.data.data);
		listDataScreen.value = data[0];
		if (data[1].length > 0) {
			detailTivi.value = data[1][0];
		}
		else {
			detailTivi.value = null;
		}
	})
    .catch((error) => {
		if (error.status === 401) {
          swal.fire({
            text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
            confirmButtonText: "OK",
          });
          store.commit("gologout");
        }
		else {
			swal.fire({
				title: "Error!",
				text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
				icon: "error",
				confirmButtonText: "OK",
			});
		}
    });
};
const selectedTivi = ref([]);
const checkDelList = ref(false);
watch(selectedTivi, () => {
	if (selectedTivi.value.length > 0) {
		checkDelList.value = true;
	} else {
		checkDelList.value = false;
	}
});
const displayAddScreen = ref(false);
const headerAddScreen = ref("");
const keyComponent = ref(0);
const addScreenTivi = () => {
	displayAddScreen.value = true;
	headerAddScreen.value = "Thêm mới màn hình";
	keyComponent.value += 1;
	screenTivi.value = { screen_name: "", tivi_id: id_tivi_active.value };
};
const removeScreen = (data) => {
	let indexD = listDataScreen.value.findIndex(x => x.screen_id == data.screen_id);
	if (indexD >= 0) {
		listDataScreen.value.splice(indexD, 1);
	}
};
const closeDialogScreen = () => {
	displayAddScreen.value = false;
	screenTivi.value = { screen_name: "", tivi_id: id_tivi_active.value };
};
const detailScreen = ref();
const id_screen_active = ref();
const configDetailScreen = (dataScreen) => {
	if (dataScreen != null) {
		id_screen_active.value = dataScreen.screen_id;
	}
	axios
		.post(
			baseUrlCheck + "api/Tivi/GetDataProc",
			{ 
				str: encr(JSON.stringify({
						proc: "tivi_get_screen_detail",
						par: [
							{ par: "user_id", va: store.getters.user.user_id },
							{ par: "screen_id", va: dataScreen != null ? dataScreen.screen_id : id_screen_active.value },
						],
					}), SecretKey, cryoptojs
				).toString()
			},
			config
    ).then((response) => {
		var data = JSON.parse(response.data.data);
		if (data[0].length > 0) {
			detailScreen.value = data[0][0];			
			selectDepartment.value = {};
        	selectDepartment.value[detailScreen.value.calendar_department_id || "-1"] = true;
			let idxScreen = listDataScreen.value.findIndex(x => x.screen_id == detailScreen.value.screen_id);
			if (idxScreen >= 0) {
				listDataScreen.value[idxScreen].content_display = detailScreen.value.content_display;
			}
		}
		else {
			detailScreen.value = null;
			selectDepartment.value = {};
			selectDepartment.value["-1"] = true;
		}
		if (data[1].length > 0) {
			data[1].forEach((el, idx) => {				
				if (!el.is_file_upload) {
            		el.thumbnail = get_youtube_thumbnail(el.link, "medium");
				}
			});
		}
		listFilesInScreen.value = data[1];
		listVideoInScreen.value = listFilesInScreen.value.length > 0 ? listFilesInScreen.value.filter(x => x.type_data == 1) : [];
		listShowsInScreen.value = listFilesInScreen.value.length > 0 ? listFilesInScreen.value.filter(x => x.type_data == 2 || x.type_data == 3) : [];
		listFileImgUploaded.value = data[2];
	})
    .catch((error) => {
		if (error.status === 401) {
          swal.fire({
            text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
            confirmButtonText: "OK",
          });
          store.commit("gologout");
        }
		else {
			swal.fire({
				title: "Error!",
				text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
				icon: "error",
				confirmButtonText: "OK",
			});
		}
    });
};

const savingConfigScreen = ref(false);
const saveConfigScreen = () => {
	if (savingConfigScreen.value == true) {
		return;
	}
	let keys = Object.keys(selectDepartment.value);
	detailScreen.value.calendar_department_id = keys[0];
	if (detailScreen.value.calendar_department_id == -1) {
		detailScreen.value.calendar_department_id = null;
	}
	if (detailScreen.value.calendar_department_id) {
		const result = getParent(treeDepartments.value, detailScreen.value.calendar_department_id, "key");
		detailScreen.value.calendar_department_id = result.key;
	}
	var formData = new FormData();
	formData.append("typeUp", "Update");
	formData.append("modelConfig", JSON.stringify(detailScreen.value));
	formData.append("listFiles", JSON.stringify(listFilesInScreen.value));
	formData.append("listImg", JSON.stringify(listFileImgUploaded.value));
	savingConfigScreen.value = true;
	swal.fire({
		width: 110,
		didOpen: () => {
			swal.showLoading();
		},
	});
	
	axios({
		method: "post",
		url: baseUrlCheck + `/api/Tivi/UpdateConfigScreen`,
		data: formData,
		headers: {
			Authorization: `Bearer ${store.getters.token}`,
		},
	})
	.then((response) => {
		swal.close();
		savingConfigScreen.value = false;
		if (response.data.err == "2") {
			swal.fire({
				title: "Thông báo",
				text: response.data.ms,
				icon: "warning",
				confirmButtonText: "OK",
			});
		} else if (response.data.err != "1") {
			toast.success("Cập nhật thiết lập màn hình thành công!");
			configDetailScreen();
		} else {
			swal.fire({
				title: "Thông báo",
				text: "Có lỗi xảy ra khi lưu thiết lập màn hình!",
				icon: "error",
				confirmButtonText: "OK",
			});
		}
	})
	.catch((error) => {
		swal.close();
		savingConfigScreen.value = false;
		swal.fire({
			title: "Error!",
			text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
			icon: "error",
			confirmButtonText: "OK",
		});
	});
};
const renderTreeDepartment = (data, id, name, title, parent_id) => {
	let arrChils = [];
	let arrtreeChils = [];
	data.filter((x) => x.parent_id == parent_id).forEach((m, i) => {
		m.IsOrder = i + 1;
		m.label_order = m.IsOrder.toString();
		let om = { key: m[id], data: m };
		const rechildren = (mm, pid) => {
			let dts = data.filter((x) => x.parent_id == pid);
			if (dts.length > 0) {
			if (!mm.children) mm.children = [];
			dts.forEach((em, index) => {
				em.label_order = mm.data.label_order + "." + (index +1);
				let om1 = { key: em[id], data: em };
				rechildren(om1, em[id]);
				mm.children.push(om1);
			});
			}
		};
		rechildren(om, m[id]);
		arrChils.push(om);
		//
		om = { key: m[id], data: m[id], label: m[name] };
		const retreechildren = (mm, pid) => {
			let dts = data.filter((x) => x.parent_id == pid);
			if (dts.length > 0) {
			if (!mm.children) mm.children = [];
			dts.forEach((em) => {
				let om1 = { key: em[id], data: em[id], label: em[name] };
				retreechildren(om1, em[id]);
				mm.children.push(om1);
			});
			}
		};
		retreechildren(om, m[id]);
		arrtreeChils.push(om);
	});
	arrtreeChils.unshift({
		key: -1,
		data: -1,
		label: "-----Chọn " + title + "----",
	});
	return { arrChils: arrChils, arrtreeChils: arrtreeChils };
};
const treeDepartments = ref([]);
const selectDepartment = ref();
selectDepartment.value = {};
selectDepartment.value[store.getters.user.organization_id] = true;
const listDepartmentsUser = () => {
	axios
		.post(
			baseUrlCheck + "api/Tivi/GetDataProc",
			{ 
				str: encr(JSON.stringify({
						proc: "tivi_get_department",
						par: [
							{ par: "user_id", va: store.getters.user.user_id },
						],
					}), SecretKey, cryoptojs
				).toString()
			},
			config
    ).then((response) => {
		var data = JSON.parse(response.data.data);
		if (data[0].length > 0) {
			let obj = renderTreeDepartment(
				data[0],
				"department_id",
				"department_name",
				"phòng ban",
				data[0][0].org_id
			);				
			treeDepartments.value = obj.arrtreeChils;
		}
		else {
			treeDepartments.value = [];
		}
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

const listFileImgUploaded = ref([]);
const listFilesInScreen = ref([]);
const listVideoInScreen = ref([]);
const listShowsInScreen = ref([]);
const menuButs = ref();
const itemButs = ref([
	{
		label: "Thêm video",
		icon: "pi pi-video",
		command: (event) => {
			openModalAddDataScreen(0);
		},
	},
	{
		label: "Thêm trình diễn",
		icon: "pi pi-images",
		command: (event) => {
			openModalAddDataScreen(1);
		},
	},
]);
const toggleAddDataScreen = (event) => {
	menuButs.value.toggle(event);
};
const showAddData = ref(false);
const titleModal = ref();
const typeList = ref();

const openModalAddDataScreen = (type) => {
	showAddData.value = false;
	typeList.value = type;
	if (type == 0) {
		titleModal.value = "Danh sách video";
		getVideoFromData();
	} else {
		titleModal.value = "Danh sách trình diễn";
		getShowsFromData();
	}
};

const closeDialog = () => {
	showAddData.value = false;
};
//get thumbnail from ytb
function get_youtube_thumbnail(url, quality) {
  if (url) {
    var video_id, thumbnail, result;
    if ((result = url.match(/youtube\.com.*(\?v=|\/embed\/)(.{11})/))) {
      video_id = result.pop();
    } else if ((result = url.match(/youtu.be\/(.{11})/))) {
      video_id = result.pop();
    }

    if (video_id) {
      if (typeof quality == "undefined") {
        quality = "high";
      }

      var quality_key = "maxresdefault"; // Max quality
      if (quality == "low") {
        quality_key = "sddefault";
      } else if (quality == "medium") {
        quality_key = "mqdefault";
      } else if (quality == "high") {
        quality_key = "hqdefault";
      }

      var thumbnail =
        "http://img.youtube.com/vi/" + video_id + "/" + quality_key + ".jpg";
      return thumbnail;
    }
  }
  return false;
}
const optionVideo = ref({
	search: "",
	pageno: 0,
	pagesize: 30,
});
const listVideoFromData = ref([]);
const getVideoFromData = () => {
	let choosen = "," + listVideoInScreen.value.map((x) => x.video_id).toString() + ",";
	axios
		.post(
			baseUrlCheck + "api/Tivi/GetDataProc",
			{ 
				str: encr(JSON.stringify({
						proc: "tivi_get_video_data",
						par: [
							{ par: "user_id", va: store.getters.user.user_id },
							{ par: "organization_id", va: store.getters.user.organization_id },
							{ par: "search", va: optionVideo.value.search },
							{ par: "pageno", va: optionVideo.value.pageno },
							{ par: "pagesize", va: optionVideo.value.pagesize },
							{ par: "listChoosen", va: choosen },
						],
					}), SecretKey, cryoptojs
				).toString()
			},
			config
    ).then((response) => {
		var data = JSON.parse(response.data.data);
		if (data[0].length > 0) {
			data[0].forEach((el, idx) => {				
				if (!el.is_file_upload) {
            		el.thumbnail = get_youtube_thumbnail(el.link, "medium");
				}
			});
		}
		listVideoFromData.value = data[0];
		optionVideo.value.totalRecords = data[1][0].total;
		optionVideo.value.loading = false;
		showAddData.value = true;
		forceRerenderTV();
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
const optionShows = ref({
	search: "",
	pageno: 0,
	pagesize: 30,
});
const listShowsFromData = ref([]);
const getShowsFromData = () => {
	let choosen = "," + listShowsInScreen.value.map((x) => x.shows_id).toString() + ",";
	axios
		.post(
			baseUrlCheck + "api/Tivi/GetDataProc",
			{ 
				str: encr(JSON.stringify({
						proc: "tivi_get_shows_data",
						par: [
							{ par: "user_id", va: store.getters.user.user_id },
							{ par: "organization_id", va: store.getters.user.organization_id },
							{ par: "search", va: optionShows.value.search },
							{ par: "pageno", va: optionShows.value.pageno },
							{ par: "pagesize", va: optionShows.value.pagesize },
							{ par: "listChoosen", va: choosen },
						],
					}), SecretKey, cryoptojs
				).toString()
			},
			config
    ).then((response) => {
		var data = JSON.parse(response.data.data);
		if (data[0].length > 0) {
			data[0].forEach((el, idx) => {				
				if (!el.is_file_upload) {
            		el.thumbnail = basedomainURL + "/Portals/Image/noimg.jpg";
				}
			});
		}
		listShowsFromData.value = data[0];
		optionShows.value.totalRecords = data[1][0].total;
		optionShows.value.loading = false;
		showAddData.value = true;
		forceRerenderTV();
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
const onPage = (event) => {
	if (typeList.value == 0) {
		optionVideo.value.pageno = event.page;
		optionVideo.value.pagesize = event.rows;
		getVideoFromData();
	}
	else {
		optionShows.value.pageno = event.page;
		optionShows.value.pagesize = event.rows;
		getShowsFromData();
	}
};
const addDataToScreen = (data, type) => {
	if (type == 0) {
		let notexist = data.filter((a) => listVideoInScreen.value.findIndex((b) => b["video_id"] === a["video_id"]) === -1);
		if (notexist.length > 0) {
			notexist.forEach((el, idx) => {
				el.link_folder = null;
				el.type_data = 1;
				el.link_org = el.path || el.link;
				el.is_embeb = el.is_file_upload ? false : true;
				el.shows_id = null;
				listVideoInScreen.value.push(el);
				listFilesInScreen.value.push(el);
			});
		}
		showAddData.value = false;
	}
	else {
		let notexist = data.filter((a) => listShowsInScreen.value.findIndex((b) => b["shows_id"] === a["shows_id"]) === -1);
		if (notexist.length > 0) {
			notexist.forEach((el, idx) => {
				el.link_folder = el.file_folder != null ? el.file_folder : null;
				el.type_data = el.file_folder != null ? 3 : 2;
				el.link_org = el.path;
				el.is_embeb = false;
				el.video_id = null;
				listShowsInScreen.value.push(el);
				listFilesInScreen.value.push(el);
			});
		}
		showAddData.value = false;
	}
};
const removeDataInScreen = (data) => {
	let indexD = -1;
	if (data.video_id != null) {
		indexD = listVideoInScreen.value.findIndex(x => x.video_id == data.video_id);
		if (indexD >= 0) {
			listVideoInScreen.value.splice(indexD, 1);
		}
		let indexF = listFilesInScreen.value.findIndex(x => x.video_id == data.video_id);
		if (indexF >= 0) {
			listFilesInScreen.value.splice(indexF, 1);
		}
	}
	else {
		indexD = listShowsInScreen.value.findIndex(x => x.shows_id == data.shows_id);
		if (indexD >= 0) {
			listShowsInScreen.value.splice(indexD, 1);
		}
		let indexF = listFilesInScreen.value.findIndex(x => x.shows_id == data.shows_id);
		if (indexF >= 0) {
			listFilesInScreen.value.splice(indexF, 1);
		}
	}
};
const componentKeyTV = ref(0);
const forceRerenderTV = () => {
	componentKeyTV.value += 1;
};
const showModalImg = ref(false);
const showModalUploadImg = () => {
	showModalImg.value = true;
};
var files = [];
const onUploadFile = (event) => {
	files = [];
	event.files.forEach((element) => {
		files.push(element);
	});
};
const removeFileUpload = (event) => {
	let idxRemove = files.findIndex(x => x.objectURL == event.file.objectURL);
	if (idxRemove >= 0) {
		files.splice(idxRemove, 1);
	}
};
const closeDialogUpload = () => {
	showModalImg.value = false;
};
const isuploading = ref(false);
const uploadFileToSys = () => {
	if (isuploading.value) {
		return;
	}
	
	let formData = new FormData();
	if (files.length == 0) {
		swal.fire({
			title: "Thông báo!",
			text: "Chọn file hình ảnh trước khi upload!",
			icon: "error",
			confirmButtonText: "OK",
		});
		return false;
	}
	for (var i = 0; i < files.length; i++) {
		let file = files[i];
		formData.append("url_file", file);
	}
	formData.append("screen_id", detailScreen.value.screen_id);
	isuploading.value = true;
	swal.fire({
		width: 110,
		didOpen: () => {
		swal.showLoading();
		},
	});
	axios({
		method: 'post',
		url: baseUrlCheck +
			`/api/Tivi/UploadFileImageScreen`,
		data: formData,
		headers: {
			Authorization: `Bearer ${store.getters.token}`,
		},
	})
	.then((response) => {
		swal.close();
		isuploading.value = false;
		if (response.data.err == "2") {
			swal.fire({
				title: "Thông báo",
				text: response.data.ms,
				icon: "warning",
				confirmButtonText: "OK",
			});
		}
		else if (response.data.err != "1") {
			toast.success("Upload ảnh thành công!");
			closeDialogUpload();
			configDetailScreen();

		} else {
			swal.fire({
				title: "Thông báo",
				text: "Xảy ra lỗi khi upload hình ảnh.",
				icon: "error",
				confirmButtonText: "OK",
			});
		}
	})
	.catch((error) => {
		swal.close();
		isuploading.value = false;
		swal.fire({
			title: "Thông báo",
			text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
			icon: "error",
			confirmButtonText: "OK",
		});
	});
};
const removeImgInScreen = (data) => {
	let indexD = -1;
	indexD = listFileImgUploaded.value.findIndex(x => x.tivi_image_id == data.tivi_image_id);
	if (indexD >= 0) {
		listFileImgUploaded.value.splice(indexD, 1);
	}
};
const menuScreenButs = ref();
const itemScreenButs = ref([
	{
		label: "Thêm mới",
		icon: "pi pi-plus-circle",
		command: (event) => {
			addScreenTivi();
		},
	},
	{
		label: "Chọn màn hình",
		icon: "pi pi-plus-circle",
		command: (event) => {
			addScreenFromDB();
		},
	},
]);
const toggleAddScreen = (event) => {
	menuScreenButs.value.toggle(event);
};

const componentScreenTV = ref(0);
const showAddScreenDB = ref(false);
const listScreenFromDB = ref([]);
const titleModalScreen = ref("");
const addScreenFromDB = () => {
	titleModalScreen.value = "Danh sách màn hình tivi";
	let choosen = "," + listDataScreen.value.map((x) => x.screen_id).toString() + ",";
	axios
		.post(
			baseUrlCheck + "api/Tivi/GetDataProc",
			{ 
				str: encr(JSON.stringify({
						proc: "tivi_list_all_screen",
						par: [
							{ par: "user_id", va: store.getters.user.user_id },
							{ par: "tivi_id", va: detailTivi.value.tivi_id },
							{ par: "listChoosen", va: choosen },
						],
					}), SecretKey, cryoptojs
				).toString()
			},
			config
    ).then((response) => {
		var data = JSON.parse(response.data.data);
		listScreenFromDB.value = data[0];
		showAddScreenDB.value = true;
		componentScreenTV.value += 1;
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
const closeDialogDBScreen = () => {
	showAddScreenDB.value = false;
};
// const addScreenToTivi = (data) => {
// 	let notexist = data.filter((a) => listDataScreen.value.findIndex((b) => b["screen_id"] === a["screen_id"]) === -1);
// 	if (notexist.length > 0) {
// 		notexist.forEach((el, idx) => {
// 			listDataScreen.value.push(el);
// 		});
// 	}
// 	showAddScreenDB.value = false;
// };

const menuImageButs = ref();
const itemImageButs = ref([
	{
		label: "Chọn ảnh",
		icon: "pi pi-plus-circle",
		command: (event) => {
			showModalSelectImg();
		},
	},
	{
		label: "Upload ảnh",
		icon: "pi pi-upload",
		command: (event) => {
			showModalUploadImg();
		},
	},
]);
const toggleAddImage = (event) => {
	menuImageButs.value.toggle(event);
};
const componentImageTV = ref(0);
const forceRerenderImage = () => {
	componentImageTV.value += 1;
};
const showAddDataImage = ref(false);
const listImagesFromData = ref([]);
const getImagesFromData = () => {
	let choosen = "," + listShowsInScreen.value.map((x) => x.shows_id).toString() + ",";
	axios
		.post(
			baseUrlCheck + "api/Tivi/GetDataProc",
			{ 
				str: encr(JSON.stringify({
						proc: "tivi_get_images_data",
						par: [
							{ par: "user_id", va: store.getters.user.user_id },
							{ par: "screen_id", va: detailScreen.value.screen_id },
							{ par: "listChoosen", va: choosen },
						],
					}), SecretKey, cryoptojs
				).toString()
			},
			config
    ).then((response) => {
		var data = JSON.parse(response.data.data);
		listImagesFromData.value = data[0];
		showAddDataImage.value = true;
		forceRerenderImage();
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
const closeDialogImage = () => {
	showAddDataImage.value = false;
};
const titleModalImage = ref("");
const showModalSelectImg = () => {
	titleModalImage.value = "Danh sách ảnh theo màn hình";
	getImagesFromData();
};
const addImageToScreen = (data) => {
	let notexist = data.filter((a) => listFileImgUploaded.value.findIndex((b) => b["tivi_image_id"] === a["tivi_image_id"]) === -1);
	if (notexist.length > 0) {
		notexist.forEach((el, idx) => {
			el.is_copy = true;
			el.group_img = el.tivi_image_id;
			el.full_name = el.full_name;
			listFileImgUploaded.value.push(el);
		});
	}
	showAddDataImage.value = false;
};

const changeDisplayCalendar = () => {
	if (detailScreen.value.display_screen_calendar) {
		detailScreen.value.display_screen_shows = false;
		detailScreen.value.display_screen_image = false;
		detailScreen.value.is_change_shows = false;
		detailScreen.value.times_changes_shows = 20;
		detailScreen.value.is_change_images = false;
		detailScreen.value.times_changes_images = 20;
	}
};
const changeDisplayShows = () => {
	if (detailScreen.value.display_screen_shows) {
		detailScreen.value.display_screen_calendar = false;
		detailScreen.value.calendar_department_id = null;
		detailScreen.value.display_calendar_meeting = false;
		detailScreen.value.display_calendar_working = false;
		detailScreen.value.display_calendar_duty = false;
		detailScreen.value.number_docs = 10;
	}
};
const changeDisplayImages = () => {
	if (detailScreen.value.display_screen_image) {
		detailScreen.value.display_screen_calendar = false;
		detailScreen.value.display_screen_docs = false;
		detailScreen.value.display_calendar_meeting = false;
		detailScreen.value.display_calendar_working = false;
		detailScreen.value.display_calendar_duty = false;
		detailScreen.value.number_docs = 10;
	}
};
onMounted(() => {
	if (!checkURL(window.location.pathname, store.getters.listModule)) {
		//router.back();
	}
	listTivi();
	listDepartmentsUser();
	return {
		listTivi,
		listDepartmentsUser,
	}
});
</script>
<template>
	<div class="main-layout h-full p-3 pt-2">
		<div class="card">
            <Splitter class="w-full" style="height:100%;">
                <SplitterPanel class="flex align-items-center justify-content-center" 
					:size="30" :minSize="30" style="max-width:30%;">
                    <div class="d-lang-table w-full h-full">
						<DataTable
							:value="filterListTivi()"
							:paginator="false"
							:scrollable="true"
							scrollHeight="flex"
							:loading="option.loading"
							v-model:selection="selectedTivi"
							selectionMode="single" 
							@rowSelect="selectRowTivi"
							:lazy="true"
							dataKey="tivi_id"
							:rowHover="true"
							:showGridlines="true"
							responsiveLayout="scroll"
							class="w-full table-screen-tivi"
							style="height: calc(100vh - 5rem) !important;"
						>
							<template #header>
								<div>
									<h3 class="m-0 pb-2">Danh sách tivi ({{(filterListTivi()).length}})</h3>
								</div>
								<div class="flex justify-content-center align-items-center" style="justify-content: space-between !important;">
									<span class="p-input-icon-left w-30rem">
										<i class="pi pi-search" />
										<InputText class="w-full" v-model="searchTivi" @keydown.enter.exact.prevent="searchScreenTV()" placeholder="Tìm kiếm ..." />
									</span>
									<!-- <Button icon="pi pi-trash" class="p-button-danger" label="Xóa TV" 
										:style="!checkDelList ? 'background-color: transparent; color: transparent; border: transparent;' : ''" 
										:disabled="!checkDelList" 
										@click="delMultiTivi()" /> -->
								</div>
							</template>
							<!-- <Column
								selectionMode="multiple"
								headerStyle="text-align:center;max-width:75px;height:50px"
								bodyStyle="text-align:center;max-width:75px;"
								class="align-items-center justify-content-center text-center"
							></Column> -->
							<Column
								field="tivi_name"
								header="Tivi"
								headerStyle="height:40px"
								bodyStyle="padding-left: 1rem !important;"
							>								
							</Column>
							<Column
								field="is_active"
								header="Kích hoạt"
								headerStyle="text-align:center;max-width:100px;height:40px"
								bodyStyle="text-align:center;max-width:100px;"
								class="align-items-center justify-content-center text-center"
							>
								<template #body="slotProps">
									<Checkbox
										:binary="true"
										v-model="slotProps.data.is_active"
										@click="onCheckBox(slotProps.data)"
									/>
								</template>
							</Column>
							<Column
								header="Chức năng"
								class="align-items-center justify-content-center text-center"
								headerStyle="text-align:center;max-width:120px;height:40px"
								bodyStyle="text-align:center;max-width:120px;"
							>
								<template #body="slotProps">
									<div>
										<Button
											@click="reloadTivi(slotProps.data)"
											class="p-button-rounded p-button-info p-button-outlined mx-1 p-0"
											type="button"
											icon="pi pi-refresh"
											v-tooltip="'Reload'"
										/>
										<Button
											@click="delSingleTivi(slotProps.data)"
											class="p-button-rounded p-button-danger p-button-outlined mx-1 p-0"
											type="button"
											icon="pi pi-trash"
											v-tooltip="'Xóa'"
										/>
									</div>
								</template>
							</Column>
							<template #empty>
								<div class="align-items-center justify-content-center p-4 text-center m-auto"
									v-if="(filterListTivi()).length == 0"
								>
									<img src="../../assets/background/nodata.png" height="144" />
									<h3 class="m-1">Không có dữ liệu</h3>
								</div>
							</template>
						</DataTable>
					</div>
                </SplitterPanel>
                <SplitterPanel :size="25" :minSize="25" style="max-width:25%;">
                    <div class="d-lang-table h-full">
						<div class="col-12 md:col-12 p-3 pb-2 flex" style="flex-direction:column;">
							<h3 class="m-0">Cấu hình tivi</h3>
						</div>
						<div class="col-12 md:col-12 p-3 py-0 flex" style="flex-direction:column;" v-if="id_tivi_active != null && detailTivi">
							<div class="col-12 px-0 class-cog-tivi">
								<label class="label-cog-tivi">Tự động chuyển màn hình tivi</label>
								<InputSwitch class="switch-cog-tivi" v-model="detailTivi.is_change_screen"/>
							</div>
							<div class="col-12 px-0 py-0 class-cog-tivi" v-if="detailTivi.is_change_screen">
								<label class="label-cog-tivi">Thời gian chuyển màn hình tivi (giây)</label>
								<InputNumber class="switch-cog-tivi" min="0" v-model="detailTivi.time_change_screen"/>
							</div>
						</div>
						<div class="field col-12 md:col-12 p-0 pt-2 flex">
							<OrderList class="order-list-screen w-full"
								v-model="listDataScreen"
								:listStyle="id_tivi_active != null ? 
									(detailTivi && detailTivi.is_change_screen ? 
										'min-height: calc(100vh - 300px);max-height: calc(100vh - 300px);'
										: 'min-height: calc(100vh - 267px);max-height: calc(100vh - 267px);') 
									: 'min-height: calc(100vh - 170px);max-height: calc(100vh - 170px);'"
								dataKey="screen_id"	
							>
								<template #header> 
									<div class="flex" style="justify-content: space-between;align-items: center;">
										<h4 class="m-0">
											Danh sách màn hình
											<span class="ml-1">{{ '(' + listDataScreen.length + ')' }}</span>
										</h4>
										<div class="flex" v-if="id_tivi_active != null">
											<Button
												class="px-2"
												label="Thêm màn hình"
												@click="toggleAddScreen"
												aria-haspopup="true"
												aria-controls="overlay_addScreenData"
											/>
											<Menu id="overlay_addScreenData"
												ref="menuScreenButs"
												:model="itemScreenButs"
												:popup="true"
												style="width: fit-content"
											/>
										</div>
									</div>
								</template>
								<template #item="slotProps">
									<div class="product-item flex" style="align-items: center;">
										<div class="product-list-detail px-1 flex-1">
											<h4 class="m-0 text-2line">
												<span class="mr-1"><i class="pi pi-desktop"></i></span>
												{{ slotProps.item.screen_name }}
											</h4>											
											<div class="product-category mt-2 pl-4" 
												style="font-size:0.9rem;"
												v-if="slotProps.item.content_display != null && slotProps.item.content_display != ''">
												{{ '(' + slotProps.item.content_display.substring(2) + ')' }}
											</div>
										</div>
										<div class="flex align-items-center justify-content-center mr-3 checkbox-display-screen" style="flex-direction: column;">
											<Checkbox class=""
												v-tooltip.top="'Hiển thị'"
												:binary="true"
												v-model="slotProps.item.is_active"
											/>
										</div>
										<div>
											<Button
												@click="configDetailScreen(slotProps.item)"
												class="p-button-rounded p-button-info p-button-outlined mr-2 p-0"
												type="button"
												style="height:2rem; width:2rem;"
												icon="pi pi-cog"
												v-tooltip="'Chi tiết'"
											/>
											<Button
												@click="removeScreen(slotProps.item)"
												class="p-button-rounded p-button-danger p-button-outlined p-0"
												type="button"
												style="height:2rem; width:2rem;"
												icon="pi pi-times"
												v-tooltip="'Xóa'"
											/>
										</div>
									</div>
								</template>
							</OrderList>
						</div>
						<div class="col-12 md:col-12 p-0 flex" style="justify-content: center;" v-if="id_tivi_active != null">
							<Button icon="pi pi-save" label="Cập nhật tivi"
								style="display: block; position: absolute; bottom: 1.5rem;" 
								@click="saveConfigTivi"></Button>
						</div>
					</div>
                </SplitterPanel>
				<SplitterPanel :size="45" :minSize="45" style="max-width:45%;">
					<div class="d-lang-table h-full">
						<div class="col-12 md:col-12 p-3 pb-2 flex" style="flex-direction:column;">
							<h3 class="m-0">Cấu hình màn hình: 
								<span class="pl-2 underline" style="color: #2196f3;" v-if="id_screen_active && detailScreen">
									{{ detailScreen.screen_name }}
								</span>
							</h3>
						</div>
						<div class="col-12 md:col-12 pl-3 pt-0 pr-2 flex" 
							style="flex-direction:column;max-height:calc(100vh - 167px);min-height:calc(100vh - 167px);overflow-y:auto;" 
							v-if="id_screen_active && detailScreen">
							<div class="col-12 pl-0 pr-3 class-cog-screen" v-if="!detailScreen.display_screen_shows && !detailScreen.display_screen_image">
								<label class="label-cog-screen label-main-screen">Hiển thị lịch</label>
								<InputSwitch class="switch-cog-screen" v-model="detailScreen.display_screen_calendar" @change="changeDisplayCalendar()" />
							</div>
							<div v-if="detailScreen.display_screen_calendar">
								<div class="col-12 px-3 class-cog-screen">
									<label class="label-cog-screen"><i class="pi pi-caret-right pr-2"></i>Hiển thị lịch họp</label>
									<InputSwitch class="switch-cog-screen" v-model="detailScreen.display_calendar_meeting"/>
								</div>
								<div class="col-12 px-3 class-cog-screen">
									<label class="label-cog-screen"><i class="pi pi-caret-right pr-2"></i>Hiển thị lịch công tác</label>
									<InputSwitch class="switch-cog-screen" v-model="detailScreen.display_calendar_working"/>
								</div>
								<div class="col-12 px-3 py-1 class-cog-screen" style="display:none !important;">
									<label class="label-cog-screen" style="width: fit-content !important; padding-right: 2rem;">
										<i class="pi pi-caret-right pr-2"></i>Phòng ban hiển thị lịch công tác
									</label>
									<TreeSelect
										class="flex-1 p-0"
										v-model="selectDepartment"
										:options="treeDepartments"
										placeholder="-- Chọn phòng ban --"
										optionLabel="data.department_name"
										optionValue="data.department_id"
									/>
								</div>
								<div class="col-12 px-3 class-cog-screen">
									<label class="label-cog-screen"><i class="pi pi-caret-right pr-2"></i>Hiển thị lịch trực ban</label>
									<InputSwitch class="switch-cog-screen" v-model="detailScreen.display_calendar_duty"/>
								</div>
								<div class="flex py-2 pr-2" style="justify-content:center;">
									<div class="w-full" style="border-bottom: 0px solid #ccc;"></div>
								</div>
							</div>
							<div class="col-12 pl-0 pr-3 class-cog-screen" v-if="!detailScreen.display_screen_shows && !detailScreen.display_screen_image">
								<label class="label-cog-screen label-main-screen">Hiển thị văn bản luật</label>
								<InputSwitch class="switch-cog-screen" v-model="detailScreen.display_screen_docs"/>
							</div>
							<div v-if="detailScreen.display_screen_docs">
								<div class="col-12 px-3 py-0 class-cog-screen">
									<label class="label-cog-screen"><i class="pi pi-caret-right pr-2"></i>Số văn bản hiển thị (tối đa 10)</label>
									<InputNumber style="width:10rem;text-align: right;" min="0" max="10" v-model="detailScreen.number_docs"/>
								</div>
								<div class="flex py-2 pr-2" style="justify-content:center;">
									<div class="w-full" style="border-bottom: 0px solid #ccc;"></div>
								</div>
							</div>
							<div class="col-12 pl-0 pr-3 class-cog-screen" v-if="!detailScreen.display_screen_calendar && !detailScreen.display_screen_docs">
								<label class="label-cog-screen label-main-screen">Hiển thị tuyên truyền</label>
								<InputSwitch class="switch-cog-screen" v-model="detailScreen.display_screen_shows" @change="changeDisplayShows()" />
							</div>
							<div v-if="detailScreen.display_screen_shows">
								<div class="col-12 px-3 class-cog-screen">
									<label class="label-cog-screen"><i class="pi pi-caret-right pr-2"></i>Tự động chuyển màn hình trình diễn</label>
									<InputSwitch class="switch-cog-screen" v-model="detailScreen.is_change_shows"/>
								</div>
								<div class="col-12 px-3 pt-0 class-cog-screen" v-if="detailScreen.is_change_shows">
									<label class="label-cog-screen"><i class="pi pi-caret-right pr-2"></i>Thời gian chuyển slide (giây)</label>
									<InputNumber style="width:10rem;text-align: right;" min="0" v-model="detailScreen.time_change_shows"/>
								</div>
								<div>
									<div class="flex align-items-center pt-1 pb-2" style="justify-content: space-between;">
										<div>
											<h4 class="mx-3 my-0">
												<i class="pi pi-caret-right pr-2"></i>Cấu hình màn hình tuyên truyền
											</h4>
										</div>
										<div class="mr-2">
											<Button
												class="p-button-sm p-button-primary ml-2 px-2 py-2"
												type="button"
												icon="pi pi-plus-circle"
												label="Thêm vào màn hình"
												@click="toggleAddDataScreen"
												aria-haspopup="true"
												aria-controls="overlay_addData"
												style="font-size: 1rem;"
											/>
											<Menu
												id="overlay_addData"
												ref="menuButs"
												:model="itemButs"
												:popup="true"
												style="width: fit-content"
											/>
										</div>
									</div>
									<div class="grid w-full m-0 pr-2" style="flex-direction:column;">
										<div class="card mb-2">
											<OrderList class="order-list-tv"
												v-model="listFilesInScreen"
												listStyle="height:auto"
												dataKey="video_id"
											>
												<template #header> 
													Danh sách file đã chọn 
													<span class="ml-1">{{ '(Video: ' + listVideoInScreen.length + ' + Trình diễn: ' + listShowsInScreen.length + ')' }}</span>
												</template>
												<template #item="slotProps">
													<div class="product-item flex" style="align-items: center;">
														<div class="image-container">
															<img v-bind:src="
																	slotProps.item.is_file_upload
																		? (slotProps.item.image
																			? basedomainURL + slotProps.item.image
																				: basedomainURL + '/Portals/Image/noimg.jpg')
																		: slotProps.item.image
																			? basedomainURL + slotProps.item.image
																				: slotProps.item.thumbnail
																"
																@error="$event.target.src = basedomainURL + '/Portals/Image/noimg.jpg'"
																:alt="slotProps.item.title"
															/>
														</div>
														<div class="product-list-detail px-3 flex-1">
															<h4 class="m-0 mb-2 text-2line">
																<span class="mr-1"><i :class="slotProps.item.type_data == 1 ? 'pi pi-video' : 'pi pi-images'"></i></span>
																{{ slotProps.item.title }}
															</h4>
															<i class="pi pi-user product-category-icon"></i>
															<span class="product-category ml-2">
																{{ slotProps.item.created_name }}
															</span>
														</div>
														<div>
															<Button icon="pi pi-times" v-tooltip.top="'Xóa'" 
																class="p-button-rounded p-button-danger p-button-outlined mx-1" 
																style="width: 2rem; height: 2rem;" 
																@click="removeDataInScreen(slotProps.item)" />
														</div>
													</div>
												</template>
											</OrderList>
										</div>
									</div>
								</div>
								<div class="flex py-2 pr-2" style="justify-content:center;">
									<div class="w-full" style="border-bottom: 0px solid #ccc;"></div>
								</div>
							</div>
							<div class="col-12 pl-0 pr-3 class-cog-screen" v-if="!detailScreen.display_screen_calendar && !detailScreen.display_screen_docs">
								<label class="label-cog-screen label-main-screen">Hiển thị hình ảnh</label>
								<InputSwitch class="switch-cog-screen" v-model="detailScreen.display_screen_image" @change="changeDisplayImages()" />
							</div>
							<div v-if="detailScreen.display_screen_image">
								<div class="col-12 px-3 class-cog-screen">
									<label class="label-cog-screen"><i class="pi pi-caret-right pr-2"></i>Tự động chuyển hình ảnh</label>
									<InputSwitch class="switch-cog-screen" v-model="detailScreen.is_change_images"/>
								</div>
								<div class="col-12 px-3 pt-0 class-cog-screen" v-if="detailScreen.is_change_images">
									<label class="label-cog-screen"><i class="pi pi-caret-right pr-2"></i>Thời gian chuyển hình ảnh (giây)</label>
									<InputNumber style="width:10rem;text-align: right;" min="0" v-model="detailScreen.time_change_images"/>
								</div>
								<div class="flex align-items-center pt-1 pb-2" style="justify-content: space-between;">
									<div>
										<h4 class="mx-3 my-0">
											<i class="pi pi-caret-right pr-2"></i>Cấu hình hình ảnh hiển thị màn hình
										</h4>
									</div>
									<!-- <div class="mr-2">
										<Button
											class="p-button-sm ml-2 px-2 py-2"
											type="button"
											icon="pi pi-plus-circle"
											label="Chọn ảnh"
											@click="showModalSelectImg"
											style="font-size: 1rem;"
										/>
										<Button
											class="p-button-sm ml-2 px-2 py-2"
											type="button"
											icon="pi pi-upload"
											label="Upload ảnh"
											@click="showModalUploadImg"
											style="font-size: 1rem;"
										/>
									</div> -->
									<div class="mr-2">
										<Button
											class="px-2"
											icon="pi pi-plus-circle"
											label="Thêm ảnh"
											@click="toggleAddImage"
											aria-haspopup="true"
											aria-controls="overlay_addImageData"
										/>
										<Menu id="overlay_addImageData"
											ref="menuImageButs"
											:model="itemImageButs"
											:popup="true"
											style="width: fit-content"
										/>
									</div>
								</div>
								<div class="grid w-full m-0 pr-2">
									<div class="field col-12 md:col-12 p-0 flex">
										<OrderList class="order-list-tv w-full"
											v-model="listFileImgUploaded"
											listStyle="height:auto"
											dataKey="tivi_image_id"
										>
											<template #header> 
												Danh sách ảnh 
												<span class="ml-1">{{ '(' + listFileImgUploaded.length + ')' }}</span>
											</template>
											<template #item="slotProps">
												<div class="product-item flex" style="align-items: center;">
													<div class="image-container">
														<img v-bind:src="
																slotProps.item.file_path
																	? (baseUrlCheck + slotProps.item.file_path)
																		: (basedomainURL + '/Portals/Image/noimg.jpg')
															"
															@error="$event.target.src = basedomainURL + '/Portals/Image/noimg.jpg'"
															:alt="slotProps.item.file_name"
														/>
													</div>
													<div class="product-list-detail px-3 flex-1">
														<h4 class="m-0 mb-2 text-2line">
															<span class="mr-1"><i class="pi pi-images"></i></span>
															{{ slotProps.item.file_name }}
														</h4>
														<i class="pi pi-user product-category-icon"></i>
														<span class="product-category ml-2">
															{{ slotProps.item.full_name }}
														</span>
													</div>
													<div class="flex align-items-center justify-content-center mr-4 checkbox-display-img" style="flex-direction: column;">
														<!-- <label class="font-bold mb-1">Hiển thị</label> -->
														<Checkbox class=""
															v-tooltip.top="'Hiển thị'"
															:binary="true"
															v-model="slotProps.item.is_active"
														/>
													</div>
													<div>
														<Button icon="pi pi-times" v-tooltip.top="'Xóa'" 
															class="p-button-rounded p-button-danger p-button-outlined mx-1"
															style="width: 2rem; height: 2rem;" 
															@click="removeImgInScreen(slotProps.item)" />
													</div>
												</div>
											</template>
										</OrderList>
									</div>
								</div>
							</div>
						</div>						
						<div class="col-12 md:col-12 p-0 flex" style="justify-content: center;border-top: 1px solid #dee2e6;" v-if="id_screen_active != null">
							<Button icon="pi pi-save" label="Cập nhật màn hình"
								style="display: block; position: absolute; bottom: 1.5rem;" 
								@click="saveConfigScreen"></Button>
						</div>
					</div>
				</SplitterPanel>
            </Splitter>
        </div>
	</div>
	<dialogScreen
		:key="keyComponent"
		:headerAddScreen="headerAddScreen"
		:displayAddScreen="displayAddScreen"
		:listScreenNow="listDataScreen"
		:closeDialog="closeDialogScreen"
		:screenTivi="screenTivi"
		:initData="selectRowTivi"
	/>
	<dialogDataScreen v-if="typeList == 0"
		:key="componentKeyTV"
		:typeList="typeList"
		:titleModal="titleModal"
		:showAddData="showAddData"
		:listDataInScreen="listVideoInScreen"
		:listDataFromData="listVideoFromData"
		:options="optionVideo"
		:onPage="onPage"
		:closeDialog="closeDialog"
		:addDataToScreen="addDataToScreen"
	/>
	<dialogDataScreen v-if="typeList == 1"
		:key="componentKeyTV"
		:typeList="typeList"
		:titleModal="titleModal"
		:showAddData="showAddData"
		:listDataInScreen="listShowsInScreen"
		:listDataFromData="listShowsFromData"
		:options="optionShows"
		:closeDialog="closeDialog"
		:onPage="onPage"
		:addDataToScreen="addDataToScreen"
	/>
	<dialogDataDBScreen
		:key="componentScreenTV"
		:titleModal="titleModalScreen"
		:showAddData="showAddScreenDB"
		:dataTivi="detailTivi"
		:listDataInScreen="listDataScreen"
		:listDataFromData="listScreenFromDB"
		:closeDialog="closeDialogDBScreen"
		:reloadListScreenTivi="selectRowTivi"
	/>
	<Dialog
		:header="'Ảnh upload'"
		v-model:visible="showModalImg"
		:autoZIndex="true"
		:modal="true"
		style="z-index: 1000"
		:style="{ width: '60vw' }"
	>
		<div class="grid formgrid m-2">			
			<div class="field col-12 md:col-12 p-0 flex">
				<div class="field col-12 md:col-12 class-file-upload">
					<FileUpload
						chooseLabel="Chọn File"
						:showUploadButton="false"
						:showCancelButton="false"
						:multiple="true"
						accept="image/*" 							
						:maxFileSize="105000000"
						@select="onUploadFile"
						@remove="removeFileUpload"
					/>
				</div>
			</div>
		</div>
		<template #footer>
			<Button
				label="Hủy"
				icon="pi pi-times"
				@click="closeDialogUpload"
				class="p-button-text"
			/>
			<Button label="Upload" icon="pi pi-check" @click="uploadFileToSys" />
		</template>
	</Dialog>
	<dialogDataImage
		:key="componentImageTV"
		:titleModal="titleModalImage"
		:showAddData="showAddDataImage"
		:listDataInScreen="listFileImgUploaded"
		:listDataFromData="listImagesFromData"
		:closeDialog="closeDialogImage"
		:addDataToScreen="addImageToScreen"
	/>
</template>
<style scoped>
	.class-cog-tivi {
		display: flex !important;
		align-items: center;
		justify-content: space-between;		
	}
	.class-cog-screen {
		display: flex !important;
		align-items: center;
		justify-content: space-between;
		padding-right: 0.5rem !important;	
	}
	.label-cog-tivi {
		text-align: left !important;
		/* width: 20rem !important; */
	}
	.label-cog-screen {
		text-align: left !important;
		width: 30rem !important;
	}
	.switch-cog-tivi {
		/* width: calc(100% - 20rem); */
		width: 7rem;
	}
	.switch-cog-screen {
		width: calc(100% - 30rem);
	}
	.label-main-screen {
		font-weight: bold;
		font-size: 1.1rem;
		color: #2196f3;
	}
	.product-item img {
		width: 6vw;
		height: 3.4vw;
		border: 1px solid #dbdee2;
	}
	.text-2line {
		text-overflow: ellipsis;
		overflow: hidden;
		column-gap: initial;
		-webkit-line-clamp: 2;
		display: -webkit-box;
		-webkit-box-orient: vertical;
	}
</style>
<style lang="scss" scoped>
	::v-deep(.btn-del-tivi) {
		.p-button-icon {
			font-size: 1.25rem;
		}
	}
	::v-deep(.table-screen-tivi) {
		.p-datatable-emptymessage td {
			border-bottom: none;
		}
		.p-datatable-thead {
			z-index: 0;
		}
	}
	::v-deep(.order-list-screen) {
		.p-orderlist-header {
			padding: 0.75rem;
		}
		.p-orderlist-list {
			padding-top: 0;
		}
		.p-orderlist-item {
			padding: 1rem 0.5rem;
			border-bottom: 1px solid #dee2e6;
		}
	}
	::v-deep(.order-list-tv) {
		.p-orderlist-list {
			padding-top: 0;
		}
	}
	::v-deep(.class-cog-tivi) {
		.p-inputnumber-input {
			text-align: right;
		}
	}
	::v-deep(.class-cog-screen) {
		.p-inputnumber-input {
			text-align: right;
		}
	}
	::v-deep(.class-file-upload) {
		.p-fileupload-content {
			padding: 1rem;
			min-height: 10rem;
		}
		button.p-button-icon-only {
			color: #fff;
			background: #D32F2F;
			border: 1px solid #D32F2F;
			border-radius: 50%;
		}
	}
	::v-deep(.checkbox-display-img) {
		.p-checkbox {
			width: 25px;
			height: 25px;
		}
		.p-checkbox .p-checkbox-box {
			width: 25px;
			height: 25px;
		}
	}
	::v-deep(.checkbox-display-screen) {
		.p-checkbox {
			width: 20px;
			height: 20px;
		}
		.p-checkbox .p-checkbox-box {
			width: 20px;
			height: 20px;
		}
	}
</style>