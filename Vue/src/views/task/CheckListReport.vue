<script setup>
    import { ref, inject, onMounted, watch } from "vue";
 
    import taskreport from "../../components/task/taskreport.vue";
 
    import moment from "moment";
    import {  checkURL } from "../../util/function.js";
//Khai báo
const router = inject("router");
    const axios = inject("axios");
    const store = inject("store");
    const swal = inject("$swal");
    const listDropdownUser = ref([]);
const listDropdownUserCheck = ref([]);
const dataChart = ref();
const isShowChart = ref(false);
const dataReport = ref([]);
const dateReport = ref();
const userReport=ref();
const isShowDialog = ref(false);
//Nơi nhận EMIT từ component
const emitter = inject("emitter");
emitter.on("emitData", (obj) => {
  switch (obj.type) {
    case "listDropdownUser":
      listDropdownUser.value = obj.data;
      break;
    case "listDropdownUserCheck":
      listDropdownUserCheck.value = obj.data;
      break;default:
      break;
  }
});
const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};
const showReport = () => {
  
  dataReport.value=[];
  axios
    .post(
      baseURL + "/api/Proc/CallProc",
      {
        proc: "task_checklist_report",
        par: [
          { par: "sdate", va:null},
          { par: "edate", va: null },
          { par: "users", va: null },
          { par: "user_id", va: store.getters.user.user_id },
        ],
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];
      let data1 = JSON.parse(response.data.data)[1];
      let data2 = JSON.parse(response.data.data)[2];
     
      userReport.value=data1;
      dateReport.value = data[0];
      data2.forEach((element) => {
        if( element.start_date!=null)
        element.start_date = moment(new Date(element.start_date)).format(
          "MM/DD/YYYY"
        );
        if( element.actual_date!=null)
        element.actual_date = moment(new Date(element.actual_date)).format(
          "MM/DD/YYYY"
        );
        if( element.created_date!=null)
        element.created_date = moment(new Date(element.created_date)).format(
          "MM/DD/YYYY"
        );
        if ((element.finished_date_save == null))
            element.finished_date_save = element.finished_date;
        if( element.finished_date!=null)
        element.finished_date = moment(new Date(element.finished_date)).format(
          "MM/DD/YYYY"
        );
        if( element.end_date!=null)
        element.end_date = moment(new Date(element.end_date)).format(
          "MM/DD/YYYY"
        );
        if( element.switch_test_date!=null)
        element.switch_test_date = moment(new Date(element.switch_test_date)).format(
          "MM/DD/YYYY"
        );
      });
      dataReport.value = data2;
      isShowDialog.value = true;
    })
    .catch((error) => {
      console.log(error);
    });
};
onMounted(() => {  if (!checkURL(window.location.pathname, store.getters.listModule)) {
     //router.back();
  }
    showReport();
})
</script>
<template>
        <div class="pt-4 overflow-auto " v-if="dataReport.length>0">
    <taskreport
      :isShowDialog="isShowDialog"
      :dataReport="dataReport"
      :dateReport="dateReport"
      :listDropdownUser="listDropdownUser"
      :userReport="userReport"
    />
  </div>
  <div
                    style="
                      translate: transform(-50%, -50%);
                      top: 50%;
                      right: 50%;
                      object-fit: cover;
                    "
                    v-else
                    class="absolute"
                  >
                    <ProgressSpinner
                      style="width: 50px; height: 50px"
                      strokeWidth="8"
                      fill="var(--surface-ground)"
                      animationDuration=".5s"
                    />
                  </div>
</template>

<style scoped>
  .color-red {
    color: red;
  }
  .bg-color-red {
    background-color: red;
  }
  table,
  th,
  td {
    border: 1px solid #e9ecef;
    border-collapse: separate;
    border-spacing: 0;
  }
  
  .m-checkbox-table {
    background-color: #fff;
    position: sticky;
    left: 0;
    top: 0;
    padding: 0px;
    min-width: 200px !important;
    max-width: 400px !important;
    text-align: center;
    border-right: none !important;
  }
  .api-test::-webkit-scrollbar {
    height: 25px !important;
  }
  </style>
  
  
  <style lang="scss" scoped>
  ::v-deep(.p-sidebar) {
    .p-sidebar-content {
      padding: 0px !important;
      overflow-y: hidden;
    }
  }
  </style>