<script  setup>
import { ref, inject, onMounted, watch } from "vue";
import { useToast } from "vue-toastification";
const axios = inject("axios");
const store = inject("store");
const swal = inject("$swal");
const emitter = inject("emitter");
const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};

//Khai báo biến (variable)
const basedomainURL = baseURL;
const toast = useToast();

 
const bgColor = ref([
  "#F8E69A",
  "#AFDFCF",
  "#F4B2A3",
  "#9A97EC",
  "#CAE2B0",
  "#8BCFFB",
  "#CCADD7",
]);
const props = defineProps({
  showSidebarUser: Boolean,
  listUsers: Object,
  title:String
});
onMounted(() => {
  listUsers.value = props.listUsers;
  showSidebarUser.value = props.showSidebarUser;
  return {};
});

const options = ref({
  IsNext: true,
  sort: "task_id",
  searchText: "",
  PageNo: 0,
  PageSize: 10,
  loading: true,
  totalRecords: null,
  finishedRecord: null,
  waitedRecord: null,
  tempClose: null,
  unFinishRecord: null,
  statusTask: null,
  outOfDate: null,
  SearchTextUser: "",
  Start_date: null,
  End_date: null,
});
const listUsers = ref([]);
const listUserShow = ref([]);
const UsersCount = ref();
const listDropdownUser = ref([]);
const listDropdownUserCheck = ref([]);
const showSidebarUser = ref(false);
const hideSidebar=()=>{
    emitter.emit("emitData", { type: "hideSidebarGU", data: null });
}
const filterTaskUser = (value) => {
  showSidebarUser.value = false;
  emitter.emit("emitData", { type: "onTaskUserFilter", data: value });
};
const listUser = (value) => {
    if(value!=""&& value!=null)
    {
    listUsers.value=props.listUsers;
    listUsers.value=listUsers.value.filter(x=>x.data.full_name.toLowerCase().indexOf(value.toLowerCase())!=-1);
  
    }else{
        listUsers.value=props.listUsers;
    }
};
</script>
<template>
  <div>
    <Sidebar
      v-model:visible="showSidebarUser"
      :baseZIndex="100"
      position="right"
      class="p-sidebar-md"
      :showCloseIcon="false"
      @hide="hideSidebar"
    >
      <div>
        <DataView
          class="w-full h-full e-sm flex flex-column"
          responsiveLayout="scroll"
          :scrollable="true"
          :layout="layout"
          :lazy="true"
          :value="listUsers"
          :loading="options.loading"
        >
          <template #header>
            <div class="grid w-full p-0">
              <div>
                <h2>{{props.title}}</h2>
              </div>
            </div>
            <Toolbar class="custoolbar">
              <template #start>
                <span class="p-input-icon-left">
                  <i class="pi pi-search" />
                  <InputText
                    type="text"
                    class="p-inputtext-sm"
                    spellcheck="false"
                    placeholder="Tìm kiếm"
                    v-model="options.SearchTextUser"
                    @keyup.enter="listUser(options.SearchTextUser)"
                  />
                </span>
              </template>
            </Toolbar>
          </template>
          <template #list="slotProps">
            <div class="grid w-full p-0">
              <div
                class="field col-12 flex m-0 cursor-pointer align-items-center"
                @click="filterTaskUser(slotProps.data)"
                :style="
                  slotProps.data.active
                    ? 'background-color:#bed3f5'
                    : 'background-color:none'
                "
              >
                <div class="col-1 p-0  align-items-center">
              
                  <Avatar
                  v-bind:label="
                        slotProps.data.data.avatar
                          ? ''
                          : slotProps.data.data.full_name.substring(
                              slotProps.data.data.full_name.lastIndexOf(' ') + 1,
                              slotProps.data.data.full_name.lastIndexOf(' ') + 2
                            )
                      "
                      :image="basedomainURL + slotProps.data.data.avatar"
                      class="w-3rem"
                      size="large"
                      :style="
                        slotProps.data.data.avatar
                          ? 'background-color: #2196f3'
                          : 'background:' +
                            bgColor[slotProps.data.data.full_name.length % 7]
                      "
                      shape="circle"
                      @error="
                        $event.target.src =
                          basedomainURL + '/Portals/Image/nouser1.png'
                      "
                  />
                </div>
                <div class="col-11 p-0 pl-2  align-items-center">
                  <div class="pt-2">
                    <div class="font-bold">
                      {{ slotProps.data.data.full_name }}
                    </div>
                    <div class="flex w-full text-sm font-italic text-500">
                      <div>{{ slotProps.data.data.user_id }}</div>
                      <div v-if=" slotProps.data.data.phone" class="">
                        <span class="px-2">|</span>{{ slotProps.data.data.phone }}
                      </div>
                    </div>
                    <div class="flex w-full text-sm font-italic text-500">
                      {{ slotProps.data.data.department_name }}
                      </div>
                  </div>
                </div>
              </div>
            </div>
          </template>
        </DataView>
      </div>
    </Sidebar>
  </div>
</template>