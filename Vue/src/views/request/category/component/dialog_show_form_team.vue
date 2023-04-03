<script setup>
import { ref, inject, onMounted } from "vue";
import { useToast } from "vue-toastification";
import { required, maxLength } from "@vuelidate/validators";
import { useVuelidate } from "@vuelidate/core";
import { encr, checkURL } from "../../../../util/function.js";
//import moment from "moment";
//import treeuser from "../../../../components/user/treeuser.vue";
const cryoptojs = inject("cryptojs");
const toast = useToast();
const axios = inject("axios");
const store = inject("store");
const swal = inject("$swal");
//const emitter = inject("emitter");
const basedomainURL = baseURL;
const baseUrlCheck = baseURL;
const config = {
	headers: { Authorization: `Bearer ${store.getters.token}` },
};
const opition = ref({
  IsNext: true,
  sort: "modified_date",
  ob: "DESC",
  PageNo: 0,
  PageSize: 20,
  search: "",
  Filteruser_id: null,
  user_id: store.getters.user_id,
  IsType: 0,
  SearchTextUser: "",
  filter_type: 0,
  sdate: null,
  edate: null,
  loctitle: "Lọc",
  type_view: 2,
  type_group_view: null,
  filter_date: null,
  filter_duan: null,
  filter_taskgroup: null,
});
const bgColor = ref([
	"#F8E69A", "#AFDFCF", "#F4B2A3", "#9A97EC", "#CAE2B0", "#8BCFFB", "#CCADD7"
]);
const props = defineProps({
	id: String,
	headerDialog: String,
	displayDialog: Boolean,
	closeDialog: Function,
});
const loadData = (rf) => {
	if (rf) {
    opition.value.loading = true;
    swal.fire({
      width: 110,
      didOpen: () => {
        swal.showLoading();
      },
    });
  }
  axios
    .post(
      baseURL + "/api/TaskProc/getTaskData",
      {
        str: encr(
          JSON.stringify({
            proc: "Srequest_Get_Khaibaoform",
            par: [
              { par: "user_id", va: store.getters.user.user_id },
              { par: "pageno", va: opition.value.PageNo },
              { par: "pagesize", va: opition.value.PageSize },
              { par: "search", va: opition.value.search },
              { par: "sort", va: opition.value.sort },
              { par: "ob", va: opition.value.ob },
              { par: "loc", va: opition.value.filter_type },
              { par: "sdate", va: opition.value.sdate },
              { par: "edate", va: opition.value.edate },
              { par: "filter_date", va: opition.value.filter_date },
            ],
          }),
          SecretKey,
          cryoptojs,
        ).toString(),
      },
      config,
    )
    .then((response) => {
      let data = JSON.parse(response.data.data);
      if (data.length > 0) {
        listData.value = concat(data[0], data[2]);
        // listData.value = data[0];
        listData.value.forEach((element, i) => {
          element.status_name = listDropdownStatus.value.filter(
            (x) => x.value == element.status,
          )[0].text;
          element.status_bg_color = listDropdownStatus.value.filter(
            (x) => x.value == element.status,
          )[0].bg_color;
          element.status_text_color = listDropdownStatus.value.filter(
            (x) => x.value == element.status,
          )[0].text_color;
          element.Thanhviens = element.Thanhviens
            ? JSON.parse(element.Thanhviens)
            : [];
          element.ThanhvienShows = [];
          if (element.Thanhviens.length > 3) {
            element.ThanhvienShows = element.Thanhviens.slice(0, 3);
          } else {
            element.ThanhvienShows = [...element.Thanhviens];
          }
          element.countThanhviens = element.Thanhviens.length;
          element.countThanhvienShows = element.ThanhvienShows.length;
          element.STT = opition.value.PageNo * opition.value.PageSize + i + 1;
          element.progress = element.count_task > 0 ? Math.floor((element.count_taskHT / element.count_task) * 100) : 0;
        });
        opition.value.type_view = type_view;
        if (type_view == 1) {
          listProjectMains.value = listData.value;
        } else if (opition.value.type_view == 2) {
          let obj = renderTreeDV(
            listData.value,
            "project_id",
            "project_name",
            "dự án",
          );
          listProjectMains.value = obj.arrChils;
          treelistProjectMains.value = obj.arrtreeChils;
        } else if (type_view == 3) {
          var listCV = groupBy(listData.value, "status");
          var arrNew = [];
          for (let k in listCV) {
            var CVGroup = [];
            listCV[k].forEach(function (r) {
              CVGroup.push(r);
            });
            arrNew.push({
              status: k,
              group_view_name: listDropdownStatus.value.filter(
                (x) => x.value == k,
              )[0].text,
              group_view_bg_color: listDropdownStatus.value.filter(
                (x) => x.value == k,
              )[0].bg_color,
              CVGroup: CVGroup,
              countProject: CVGroup.length,
            });
          }
          listProjectMains.value = arrNew;
          // stt.value = data[1][0].total + 1;
        } else if (type_view == 4 || type_view == 5) {
          listProjectMains.value = listData.value;
          let date1 = new Date(
            opition.value.sdate ? opition.value.sdate : new Date(),
          );
          let date2 = new Date(
            opition.value.edate ? opition.value.edate : new Date(),
          );
          // var firstDay = new Date(date.getFullYear(), date.getMonth(), 1);
          // var lastDay = new Date(date.getFullYear(), date.getMonth() + 1, 0);
          var firstDay = new Date(date1.getFullYear(), date1.getMonth(), 1);
          var lastDay = new Date(date2.getFullYear(), date2.getMonth() + 1, 0);
          getDates(firstDay, lastDay);
        }
        opition.value.totalRecords = data[1][0].totalrecords;
      } else {
        listProjectMains.value = [];
      }
      if (rf) {
        opition.value.loading = false;
        swal.close();
      }
    })
    .catch((error) => {
      toast.error("Tải dữ liệu không thành công!");
      opition.value.loading = false;
      addLog({
        title: "Lỗi Console loadData",
        controller: "LogsView.vue",
        log_content: error.message,
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
}
onMounted(() => {
	// loadData(true);
	return {};
});
</script>
<template>
	<Dialog :header="props.headerDialog" v-model:visible="props.displayDialog" :style="{ width: '70vw' }" :closable="false"
		:modal="true">
		<form @submit.prevent="">
			<div class="grid formgrid m-0">
				<div class="field col-12 md:col-12 algn-items-center flex p-0">
					<div class="col-6 text-left flex p-0" style="align-items:center;">
						<span class="p-input-icon-left">
							<i class="pi pi-search" />
							<InputText type="text" spellcheck="false" placeholder="Tìm kiếm" style="min-width:30rem;" />
						</span>
					</div>
					<div class="col-6 text-left flex p-0" style="align-items:center;justify-content: end;">
						<span class="p-input-icon-left">
							<i class="pi pi-search" />
							<Button @click="addTask('Chọn team')" label="Chọn team" icon="pi pi-users" class="mr-2" />
						</span>
					</div>
				</div>
				<div class="field col-12 md:col-12 algn-items-center flex p-0">
					
				</div>
			</div>
		</form>
		<template #footer>
			<Button label="Hủy" icon="pi pi-times" @click="props.closeDialog" class="p-button-outlined" />
			<Button label="Lưu" icon="pi pi-check" @click="saveData()" autofocus />
		</template>
	</Dialog>
</template>
<style scoped></style>