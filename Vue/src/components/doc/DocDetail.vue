<script setup>
import { defineProps, onMounted, ref, inject } from "vue";
import {formatDate} from "../../util/function";
import { useToast } from "vue-toastification";
import DocOrgChart from "./DocOrgChart.vue";
import DocMessage from "./DocMessage.vue";
import { encr } from "../../util/function";
const cryoptojs = inject("cryptojs");
const axios = inject("axios"); // inject axios
const store = inject("store");
const swal = inject("$swal");
const toast = useToast();
const emitter = inject("emitter");
const router = inject("router");
const basedomainURL = fileURL;
const config = {
    headers: { Authorization: `Bearer ${store.getters.token}` },
};
const props = defineProps({
  goToDetailSohoa: Function,
  goToDetailDuthao: Function,
  goToDetailStamp: Function,
  goToViewDoc: Function,
  Type: String
});
// defined variable
const isLoading = ref(false);
const displayModalIframeDoc = ref(false);
const isLoaded = ref(false);
const DocRelUsers = ref([]);
const DetailDocItem = ref({});
// get data doc
emitter.on("emitData", (obj) => {
    switch (obj.type) {
        case "goToDetailDoc":
            if (obj.data) {
                DetailDocItem.value = obj.data;
                isLoaded.value = true;
                loadUserRelDoc();
                loadDetailDoc(true);
                loadDocFile();
                loadRelatedDoc();
                DetailDocItem.value.is_not_seen = props.Type === 'receive' && (!DetailDocItem.value.view_id || (!DetailDocItem.value.view_date && DetailDocItem.value.status_id !== DetailDocItem.value.first_doc_status_id));
            }
            break;
        case "updateViewDoc":
        updateViewDoc(DetailDocItem.value, true);
        break;
        default: break;
    }
});
const loadUserRelDoc = () => {
    axios
        .post(
            baseURL + "/api/DocProc/CallProc",
            {str: 
              encr(JSON.stringify(
              {
                proc: "doc_master_listuser_reldoc",
                par: [
                    { par: "doc_master_id", va: DetailDocItem.value.doc_master_id },
                ],
            }
            ),
            SecretKey, cryoptojs)
            .toString()
            },            
            config
        )
        .then((response) => {
            let data = JSON.parse(response.data.data)[0];
            if (data.length > 0) {
                DocRelUsers.value = data;
                DocRelUsers.value.forEach((ele) => {
                  if(ele.viewed_date) ele.viewed_date = formatDate(ele.viewed_date, 'datetime');
                });
            } else {
                DocRelUsers.value = [];
            }
        })
        .catch((error) => {
            if (error && error.status === 401) {
                swal.fire({
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
            }
        });
};
const headerUserRel = ref();
const displayUserRel = ref(false);
const openModalViewUserRelDoc = () => {
  headerUserRel.value = "Nhân sự xử lý và nhận văn bản";
  displayUserRel.value = true;
}
// doc file
const docfiles = ref([]);
const menuDocFiles = ref();
const toggleDocFile = (event) => {
  menuDocFiles.value.toggle(event);
};
const loadDocFile = () => {
    axios
        .post(
            baseURL + "/api/DocProc/CallProc",
            {str: 
              encr(JSON.stringify(
              {
                proc: "doc_file_list",
                par: [
                    { par: "doc_master_id", va: DetailDocItem.value.doc_master_id },
                ],
            }
            ),
            SecretKey, cryoptojs)
            .toString()
            },  
            config
        )
        .then((response) => {
            let data = JSON.parse(response.data.data)[0];
            if (data.length > 0) {
              docfiles.value = data;
            } else {
              docfiles.value = [];
            }
        })
        .catch((error) => {
            if (error && error.status === 401) {
                swal.fire({
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
            }
        });
};
// related doc
const related_docs = ref([]);
const loadRelatedDoc = () => {
    axios
        .post(
            baseURL + "/api/DocProc/CallProc",
            {str: 
              encr(JSON.stringify(
              {
                proc: "doc_master_related_list",
                par: [
                    { par: "doc_master_id", va: DetailDocItem.value.doc_master_id },
                ],
            }
            ),
            SecretKey, cryoptojs)
            .toString()
            },  
            config
        )
        .then((response) => {
            let data = JSON.parse(response.data.data)[0];
            if (data.length > 0) {
              related_docs.value = data;
            } else {
              related_docs.value = [];
            }
        })
        .catch((error) => {
            if (error && error.status === 401) {
                swal.fire({
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
            }
        });
};
const menuRelatedDoc = ref();
const toggleRelatedDoc = (event) => {
  menuRelatedDoc.value.toggle(event);
};
// update view doc
const updateViewDoc = (docitem, is_hide) => {
  if (docitem.view_date && docitem.view_id) {
    swal.close();
    return;
  }
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  axios({
    method: "put",
    url:
      baseURL +
      `/api/DocMain/Update_ViewDoc`,
    data: {doc_master_id: docitem.doc_master_id, follow_id: docitem.view_date ? null : docitem.follow_id},
    headers: {
      Authorization: `Bearer ${store.getters.token}`,
    },
  })
    .then((response) => {
      if (response.data.err != "1") {
        swal.close();
        if(!is_hide){
          toast.success("Cập nhật đã xem văn bản thành công!");
          DetailDocItem.value.is_not_seen = false;
          loadUserRelDoc();
          emitter.emit("emitData", { type:"refreshDocAfterViewed", data: null });
        }
      } else {
        swal.fire({
          title: "Thông báo!",
          text: response.data.ms,
          icon: "error",
          confirmButtonText: "OK",
        });
      }
    })
    .catch((error) => {
      swal.close();
      swal.fire({
        title: "Error!",
        text: "Có lỗi xảy ra, vui lòng kiểm tra lại!",
        icon: "error",
        confirmButtonText: "OK",
      });
    });
};
//
const loadDetailDoc = (rf) => {
    if (rf) {
    isLoading.value = true;
    swal.fire({
      width: 110,
      didOpen: () => {
        swal.showLoading();
      },
    });
    // if (options.value.PageNo == 1) loadCount();
  }
  axios
    .post(
      baseURL + "/api/DocProc/CallProc",
      {str: 
        encr(JSON.stringify(
        {
          proc: "doc_master_get_detail",
          par: [
            { par: "doc_master_id", va: DetailDocItem.value.doc_master_id },
          ],
        }
      ),
        SecretKey, cryoptojs)
        .toString()
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];
      if (data.length > 0) {
        data.forEach(function (r) {
          if(r.receive_date) r.receive_date = formatDate(r.receive_date,'date');
        });
        DetailDocItem.value = {...DetailDocItem.value, ...data[0]};
      } 
      if (rf) {
        isLoading.value = false;
        swal.close();
      }
    })
    .catch((error) => {
      if (error && error.status === 401) {
        swal.fire({
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
      }
    });
}
const selectedFileDoc = ref({})
const openModalIframeDoc = (file)=>{
  selectedFileDoc.value = file;
  if(selectedFileDoc.value.compendium){
    selectedFileDoc.value.display_name = selectedFileDoc.value.compendium;
  }
  else{
    selectedFileDoc.value.display_name = selectedFileDoc.value.file_name;
  }
    displayModalIframeDoc.value = true;
}
const displayCompleted = ref();
const headerCompleted = ref();
const completed_item = ref({});
const listFileUploaded = ref([]); 
const listFileDel = ref([]);
const openModalViewCompleted = () => {
  headerCompleted.value = "Xem file xác nhận hoàn thành công việc văn bản";
  displayCompleted.value = true;
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  axios
    .post(
      baseURL
      //baseUrl
      + "/api/DocProc/CallProc",
      {str: 
        encr(JSON.stringify(
        {
        proc: "doc_follow_get_completed_doc",
        par: [
          { par: "follow_id", va: DetailDocItem.value.follow_id },
        ],
      }
      ),
        SecretKey, cryoptojs)
        .toString()
      },
      config
    )
    .then((response) => {
      swal.close();
      let data = JSON.parse(response.data.data);
      if (data[0]) {
        completed_item.value = data[0][0];
      }
      data[1].forEach((element, i) => {
        element.file_type = element.file_type.toLowerCase();
      });
      listFileUploaded.value = data[1];
      listFileDel.value = data[1];
    })
    .catch((error) => {
      toast.error("Tải dữ liệu không thành công!");
      swal.close();
      addLog({
        title: "Lỗi Console loadData",
        controller: "SQLView.vue",
        logcontent: error.message,
        loai: 2,
      });
      if (error && error.status === 401) {
        swal.close();
        swal.fire({
          text: "Mã token đã hết hạn hoặc không hợp lệ, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
      }
    });
}
const closeDialog = (type_doc) => {
  switch (type_doc) {
    case 'xacnhanhoanthanh':
      displayCompleted.value = false;
      case 'relusers':
      displayUserRel.value = false;
  }
};
onMounted(() => {
    //init
    return {

    };
});
</script>

<template>
    <div class="doc-detail" id="splitter-docdetail">
        <TabView lazy>
            <TabPanel>
                <template #header>
                    <i class="pi pi-file mr-2"></i>
                    <span>Nội dung</span>
                </template>
                <div v-if="isLoaded" class="doc-content">
                    <div class="flex justify-content-between">
                        <h3>{{ DetailDocItem.compendium }}</h3>
                        <div class="flex">
                            <Button v-if="DetailDocItem.is_completed" @click="openModalViewCompleted" v-tooltip.left="'Nội dung xác nhận hoàn thành'" icon="pi pi-file" style="font-size: 2rem;background-color:transparent;border:none" class="p-button-text" />
                            <Function v-if="Type !== 'send'">
                              <Button v-if="(Type=== 'store' && !DetailDocItem.is_drafted) || (DetailDocItem.status_id === 'sohoa' || (DetailDocItem.status_id === 'tralai' && store.getters.user.user_key === DetailDocItem.created_by && DetailDocItem.first_status_id === 'sohoa'))" 
                            @click="props.goToDetailSohoa(DetailDocItem)" 
                            v-tooltip.left="'Chỉnh sửa'" 
                            icon="pi pi-pencil" 
                            style="font-size: 2rem;background-color:transparent;border:none" 
                            class="p-button-text" />
                            <Button v-if="(Type === 'store' && DetailDocItem.is_drafted) || ((DetailDocItem.status_id === 'duthao' || (DetailDocItem.status_id === 'tralai' && store.getters.user.user_key === DetailDocItem.created_by && DetailDocItem.first_status_id === 'duthao') || (DetailDocItem.status_id === 'xulychinh' && DetailDocItem.first_status_id === 'duthao')) && (DetailDocItem.nav_type === 2 || DetailDocItem.nav_type === 3))" 
                            @click="props.goToDetailDuthao(DetailDocItem)" 
                            v-tooltip.left="'Chỉnh sửa'" 
                            icon="pi pi-pencil" 
                            style="font-size: 2rem;background-color:transparent;border:none" 
                            class="p-button-text" />
                            <Button v-if="Type !== 'store' && DetailDocItem.status_id === 'dadongdau'" 
                            @click="props.goToDetailStamp(DetailDocItem)" 
                            v-tooltip.left="'Chỉnh sửa'" 
                            icon="pi pi-pencil" 
                            style="font-size: 2rem;background-color:transparent;border:none" 
                            class="p-button-text" />
                            </Function>
                            <Button @click="toggleRelatedDoc" v-if="related_docs.length > 0" v-tooltip.left="'Văn bản liên quan (' + related_docs.length + ')'" icon="pi pi-clone" style="font-size: 2rem;background-color:transparent;border:none" class="p-button-text" />
                          <Menu class="file-menu" :model="related_docs" ref="menuRelatedDoc" :popup="true">
                                <template #item="{ item }">
                                  <a @click="openModalIframeDoc(item)" style="color: inherit" class="w-full no-underline">
                                    <div class="flex align-items-center">
                                      <img class="mr-2" :src="basedomainURL + '/Portals/Image/file/' + item.file_type + '.png'"
                                        style="object-fit: contain;" width="24" height="24" />
                                      <span style="line-height:1.5"> {{ item.file_name }}</span>
                                    </div>
                                  </a>
                                </template>
                            </Menu>
                        <Button @click="toggleDocFile" v-if="docfiles.length > 0" v-tooltip.left="'Đính kèm khác (' + docfiles.length + ')'" icon="pi pi-paperclip" style="font-size: 2rem;background-color:transparent;border:none" class="p-button-text" />
                          <Menu class="file-menu" :model="docfiles" ref="menuDocFiles" :popup="true">
                                <template #item="{ item }">
                                  <a @click="openModalIframeDoc(item)" style="color: inherit" class="w-full no-underline">
                                    <div class="flex align-items-center">
                                      <img class="mr-2" :src="basedomainURL + '/Portals/Image/file/' + item.file_type + '.png'"
                                        style="object-fit: contain;" width="24" height="24" />
                                      <span style="line-height:1.5"> {{ item.file_name }}</span>
                                    </div>
                                  </a>
                                </template>
                            </Menu>
                          <Button v-if="DetailDocItem.status_id !== 'sohoa' && DetailDocItem.status_id !== 'duthao'" @click="props.goToViewDoc(DetailDocItem)" v-tooltip.left="'Xem chi tiết'" icon="pi pi-eye" style="font-size: 2rem;background-color:transparent;border:none" class="p-button-text" />
                          <Button @click="openModalIframeDoc(DetailDocItem)" v-tooltip.left="'Xem toàn màn hình'" icon="pi pi-window-maximize" style="font-size: 2rem;background-color:transparent;border:none" class="p-button-text" />
                        </div>
                         <Sidebar v-model:visible="displayModalIframeDoc" :baseZIndex="10000" position="full">
                           <iframe v-if="selectedFileDoc.file_path" style="height: calc(100vh - 3.2rem)"  :src="basedomainURL + '/Viewer?title=' + selectedFileDoc.file_name + '&url=' + selectedFileDoc.file_path" class="w-full"></iframe>
                        </Sidebar>
                    </div>
                    <div style="max-width: 80%">
                        <div class="flex justify-content-between">
                            <div>
                                <div>Nhóm văn bản: <b>{{ DetailDocItem.doc_group }}</b></div>
                                <div class="mt-3">{{DetailDocItem.nav_type === 1 ? 'Ngày đến' : (DetailDocItem.nav_type === 2 ? 'Ngày đi' : 'Ngày')}}: <b>{{ DetailDocItem.receive_date }}</b></div>
                            </div>
                            <div>
                                <div>Cơ quan ban hành: <b>{{ DetailDocItem.issue_place }}</b></div>
                                <div class="mt-3">Lĩnh vực: <b>{{ DetailDocItem.field_name }}</b></div>
                            </div>
                        </div>
                    </div>
                    <div v-if="DetailDocItem.message" class="mt-3 flex justify-content-center">
                        <Chip class="custom-chip" :label="DetailDocItem.message" icon="pi pi-comments" />
                    </div>
                    <div class="mt-3 doc-viewer">
                        <iframe v-if="DetailDocItem.file_path" :src="basedomainURL + '/Viewer?title=' + DetailDocItem.compendium + '&url=' + DetailDocItem.file_path" class="w-full"></iframe>
                        <div class="no-file-text" v-if="!DetailDocItem.file_path"><h1 style="color: #999999">Không có file để hiển thị</h1></div>
                      </div>
                </div>
            </TabPanel>
            <TabPanel>
                <template #header>
                    <i class="pi pi-comments mr-2"></i>
                    <span>Ý kiến/Chỉ đạo</span>
                </template>
                <DocMessage v-if="!isLoading" :DocItem="DetailDocItem"></DocMessage>
            </TabPanel>
            <TabPanel>
                <template #header>
                    <i class="pi pi-share-alt mr-2"></i>
                    <span>Tiến trình xử lý</span>
                </template>
                <DocOrgChart class="auto-width" v-if="!isLoading" :Type="'all'" :DocItem="DetailDocItem"></DocOrgChart>
            </TabPanel>
        </TabView>
        <div class="flex detail-right-top" style="column-gap: 1rem">
          <AvatarGroup style="cursor: pointer" @click="openModalViewUserRelDoc">
            <Avatar v-for="item in DocRelUsers.slice(0, 5)" :key="item.user_id" v-tooltip.left="item.full_name"
                v-bind:label="item.avatar ? '' : item.full_name.split(' ').at(-1).substring(0, 1).toUpperCase()"
                v-bind:image="basedomainURL + item.avatar"
                style="background-color: #2196f3; color: #ffffff; vertical-align: middle" class="mr-2" size="small"
                shape="circle" />
            <Avatar v-if="DocRelUsers && DocRelUsers.length > 5"
                v-bind:label="'+' + (DocRelUsers.length - 5).toString()" shape="circle" size="small"
                style="background-color: #2196f3; color: #ffffff" />
        </AvatarGroup>
        <Button v-if="DetailDocItem.is_not_seen" @click="updateViewDoc(DetailDocItem)" label="Đã xem" icon="pi pi-check" class="p-button-success btn-viewed-doc" />
        </div>
    </div>
    <Dialog :modal="true" :header="headerCompleted" v-model:visible="displayCompleted" :autoZIndex="true"
    :style="{ width: '50vw' }">
    <form>
      <div class="grid formgrid m-2">
        <div class="field col-12 md:col-12 flex">
          <div class="col-12 md:col-12 m-0 p-0">
            <label class="col-4 p-0 text-left flex" style="align-items:center;">
              Nội dung
            </label>
            <Textarea v-model="completed_item.completed_message" spellcheck="false" class="col-12 ip36" autoResize autofocus
              style="padding:0.5rem;" />
          </div>
        </div>
        <div class="col-12 md:col-12">
          <div class="field col-12 md:col-12 p-0 flex" v-if="listFileUploaded.length > 0">
            <label class="col-2 p-0 mt-3 text-left">File đính kèm</label>
            <div class="col-10 p-0 item-file-law">
              <DataView class="w-full h-full ptable p-datatable-sm flex flex-column" :lazy="true"
                :value="listFileUploaded" layout="list" :rowHover="true" responsiveLayout="scroll" :scrollable="true">
                <template #list="slotProps">
                  <div class="w-full">
                    <Toolbar class="w-full">
                      <template #start>
                        <a target="_blank" :href="basedomainURL + slotProps.data.file_path" download class="w-full no-underline">
                            <div class="flex align-items-center">
                                <img class="mr-2" :src="basedomainURL + '/Portals/Image/file/' + slotProps.data.file_type + '.png'"
                                    style="object-fit: contain;" width="40" height="40" />
                                <span style="line-height:1.5"> {{ slotProps.data.file_name }}</span>
                            </div>
                        </a>
                      </template>
                    </Toolbar>
                  </div>
                </template>
              </DataView>
            </div>
          </div>
        </div>
      </div>
    </form>
    <template #footer>
      <Button label="Hủy" icon="pi pi-times" @click="closeDialog('xacnhanhoanthanh')" class="p-button-text" />
    </template>
    </Dialog>
    <Dialog :modal="true" :header="headerUserRel" v-model:visible="displayUserRel" :autoZIndex="true"
    :style="{ width: '35vw' }">
    <form>
      <div class="grid formgrid m-2">
        <div class="field col-12 md:col-12 flex">
          <div class="col-12 md:col-12 m-0 p-0">
            <div style="justify-content: space-between;" v-for="item in DocRelUsers" :key="item.user_id" class="user-item  flex">
              <div class="flex">
                  <Avatar 
                  v-bind:label="item.avatar ? '' : item.full_name.split(' ').at(-1).substring(0, 1).toUpperCase()"
                v-bind:image="basedomainURL + item.avatar"
                style="background-color: #2196f3; color: #ffffff; vertical-align: middle"
                  class="mr-2" size="large" shape="circle" />
                  <div style="line-height: 1.5" class="pt-1">
                    <div><strong>{{ item.full_name }}</strong></div>
                    <div style="color: #aaa; font-weight: 500">{{ item.organization_name }}</div>
                    <div style="color: #aaa; "><span v-if="item.position_name">{{item.position_name + ' | '}}</span> {{ item.department_name }}</div>
                  </div>
              </div>
              <span v-if="item.viewed_date" style="user-select: none; color:#9a9a9a; font-size: 12px;">{{item.viewed_date}}</span>
              <Chip v-if="!item.viewed_date" v-bind:label="'Chưa xem'" style="background-color: red; color: #fff; border-radius: 5px; font-size: 11px;" />
            </div>
            
          </div>
        </div>
      </div>
    </form>
    <template #footer>
      <Button label="Hủy" icon="pi pi-times" @click="closeDialog('relusers')" class="p-button-text" />
    </template>
    </Dialog>
</template>
<style lang="scss" scoped>
.detail-right-top{
  position: absolute;
    right: 10px;
    top: 6px;
}
.doc-detail {
    position: relative;
}

// ::v-deep(.p-avatar-group) {
//     position: absolute;
//     right: 8rem;
//     top: 10px;
// }

::v-deep(.p-tabview) {
    .p-tabview-panels {
        padding: 0 1rem
    }
}
.no-file-text{
    display: flex;
    justify-content: center;
    align-items: center;
    height: calc(100vh - 20rem);
}
.p-chip.custom-chip {
    background: #dbf1ff;
    color: #000;
}
.doc-viewer iframe{
    min-height: calc(100vh - 23rem);
    border: none;
}
</style>
<style>
  .file-menu .p-menu-list .p-menuitem {
    padding: 5px 10px !important;
  }

  .file-menu .p-menu-list .p-menuitem:hover {
      cursor: pointer !important;
      background-color: #e9ecef !important;
  }
  .user-item{
    padding: 10px 0;
    border-bottom: 1px solid rgb(242, 242, 242);
  }
</style>