<script setup>
import { ref, inject, onMounted, watch } from "vue";
import { useToast } from "vue-toastification";
import moment from "moment";import { encr } from "../../util/function.js";
const basedomainURL = baseURL;
const emitter = inject("emitter");
const axios = inject("axios");const cryoptojs = inject("cryptojs");
const store = inject("store");
const swal = inject("$swal");
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
const props = defineProps({
  device: Object,
  type: String,
  device_process_code: String,
  approved_group_id: Intl,
  isdefault: Boolean,
  device_note_id: Intl,
  checkApp: Intl,
  device_process_id: Intl,
  device_process_type: Intl,
  listAssetsH: [],
});
const toast = useToast();
const options = ref({
  IsNext: true,
  sort: " approved_group_id DESC",
  search: "",
  pageno: 0,
  pagesize: 100000,
  loading: true,
  totalRecords: null,
  totalRecordsDM: null,
  start_date: null,
  end_date: null,
  next: true,
});
const datalists = ref();
const filterSQL = ref();
const files = ref([]);
const filterSCard = ref();
const filterTCard = ref();
const listSCard = ref([
  { name: "Duyệt phòng ban", code: "1" },
  { name: "Duyệt cấp hành chính, lãnh đạo", code: "2" },
  { name: "Kiểm kho thiết bị", code: "3" },
  { name: "Mua sắm thiết bị", code: "4" },
  { name: "Duyệt mua sắm thiết bị", code: "5" },
  { name: "Xác nhận mua sắm thiết bị", code: "6" },
  { name: "Lập thẻ thiết bị", code: "7" },
  { name: "Phòng chức năng duyệt", code: "8" },
  { name: "Phòng chức năng đánh giá", code: "9" },
  { name: "Phòng kỹ thuật duyệt", code: "10" },
  { name: "Phòng kỹ thuật đánh giá", code: "11" },
  { name: "Báo giá sửa chữa", code: "12" },
  { name: "Lãnh đạo duyệt phí sửa chữa", code: "13" },
  { name: "Hoàn thành sửa chữa", code: "14" },
  { name: "Kế toán duyệt", code: "15" },
  { name: "Lãnh đạo duyệt", code: "16" },
  { name: "Hoàn thành", code: "17" },
  { name: "Xác nhận hoàn thành", code: "18" },
]);
const listTCard = ref([
  { name: "Duyệt một nhiều", code: 1 },
  { name: "Duyệt tuần tự", code: 2 },
  { name: "Duyệt ngẫu nhiên", code: 3 },
]);
let fileSize = [];
const onUploadFile = (event) => {
  fileSize = [];
   files.value=[];
  event.files.forEach((element) => {
    files.value.push(element);
    fileSize.push(element.size);
  });
};
const removeFile = (event) => {
  files.value = files.value.filter((a) => a != event.file);
};
const toggleApproved = (event) => {
  filterApproved.value.toggle(event);
};
const filterApproved = ref();
const filterCard = () => {
  datalists.value = [];
  axios
  .post(
      baseURL + "/api/device_card/getData",
      {
        str: encr(
          JSON.stringify({
        proc: "device_aprroved_dropdown",
        par: [
          { par: "user_id", va: store.getters.user.user_id },
          { par: "type", va: props.type },
        ],
      }),
            SecretKey,
            cryoptojs
          ).toString(),
        },config
    )
    .then((response) => {
      let dt = JSON.parse(response.data.data);

      let data = dt[0];

      if (data.length > 0) {
        data.forEach((element, i) => {
          element.is_order =
            options.value.pageno * options.value.pagesize + i + 1;
          if (!element.config_type_name) {
            element.config_type_name =
              element.classify == 1
                ? "Duyệt phòng ban"
                : element.classify == 2
                ? "Duyệt cấp hành chính, lãnh đạo"
                : element.classify == 3
                ? "Kiểm kho thiết bị"
                : element.classify == 4
                ? "Mua sắm thiết bị"
                : element.classify == 5
                ? "Duyệt mua sắm thiết bị"
                : element.classify == 6
                ? "Xác nhận mua sắm thiết bị"
                : element.classify == 7
                ? "Lập thẻ thiết bị"
                : element.classify == 8
                ? "Phòng chức năng duyệt"
                : element.classify == 9
                ? "Phòng chức năng đánh giá"
                : element.classify == 10
                ? "Phòng kỹ thuật duyệt"
                : element.classify == 11
                ? "Phòng kỹ thuật đánh giá"
                : element.classify == 12
                ? "Báo giá sửa chữa"
                : element.classify == 13
                ? "Lãnh đạo duyệt phí sửa chữa"
                : element.classify == 14
                ? "Hoàn thành sửa chữa"
                : element.classify == 15
                ? "Kế toán duyệt"
                : element.classify == 16
                ? "Lãnh đạo duyệt"
                : element.classify == 17
                ? "Hoàn thành"
                : "Xác nhận hoàn thành";
          }
          element.device_approved_li = JSON.parse(element.device_approved_li);
          if (props.checkApp == 1 && element.classify != 17) {
            datalists.value.push({
              name: element.approved_group_name,
              code: element.approved_group_id,
              data: element,
              users: element.device_approved_li,
            });
          }
          if (props.checkApp == 2) {
            datalists.value.push({
              name: element.approved_group_name,
              code: element.approved_group_id,
              data: element,
              users: element.device_approved_li,
            });
          }
        });
      }

      if (filterTCard.value != null) {
        datalists.value = datalists.value.filter(
          (x) => x.data.approved_type == filterTCard.value
        );
      }
      if (filterSCard.value != null) {
        datalists.value = datalists.value.filter(
          (x) => x.data.classify == filterSCard.value
        );
      }
      filterApproved.value.hide();
      isRefAprroved.value.show();
      options.value.loading = false;
    })
    .catch((error) => {
      console.log(error);
      options.value.loading = false;
      toast.error("Tải dữ liệu không thành công!");
    });
};
const reFilterCard = () => {
  filterSCard.value = null;
  filterTCard.value = null;
  loadData();
  filterApproved.value.hide();
  isRefAprroved.value.show();
};
const loadData = () => {
  filterSQL.value = [];

  let filterS = {
    filterconstraints: [
      { value: props.type, matchMode: "equals" },
      { value: "all", matchMode: "equals" },
    ],
    filteroperator: "or",
    key: "module",
  };
  filterSQL.value.push(filterS);
  datalists.value = [];
  options.value.loading = true;
  axios
  .post(
      baseURL + "/api/device_card/getData",
      {
        str: encr(
          JSON.stringify({
        proc: "device_aprroved_dropdown",
        par: [
          { par: "user_id", va: store.getters.user.user_id },
          { par: "type", va: props.type },
        ],
      }),
            SecretKey,
            cryoptojs
          ).toString(),
        },config
    )
    .then((response) => {
      let dt = JSON.parse(response.data.data);

      let data = dt[0];

      if (data.length > 0) {
        data.forEach((element, i) => {
          element.is_order =
            options.value.pageno * options.value.pagesize + i + 1;
          if (!element.config_type_name) {
            element.config_type_name =
              element.classify == 1
                ? "Duyệt phòng ban"
                : element.classify == 2
                ? "Duyệt cấp hành chính, lãnh đạo"
                : element.classify == 3
                ? "Kiểm kho thiết bị"
                : element.classify == 4
                ? "Mua sắm thiết bị"
                : element.classify == 5
                ? "Duyệt mua sắm thiết bị"
                : element.classify == 6
                ? "Xác nhận mua sắm thiết bị"
                : element.classify == 7
                ? "Lập thẻ thiết bị"
                : element.classify == 8
                ? "Phòng chức năng duyệt"
                : element.classify == 9
                ? "Phòng chức năng đánh giá"
                : element.classify == 10
                ? "Phòng kỹ thuật duyệt"
                : element.classify == 11
                ? "Phòng kỹ thuật đánh giá"
                : element.classify == 12
                ? "Báo giá sửa chữa"
                : element.classify == 13
                ? "Lãnh đạo duyệt phí sửa chữa"
                : element.classify == 14
                ? "Hoàn thành sửa chữa"
                : element.classify == 15
                ? "Kế toán duyệt"
                : element.classify == 16
                ? "Lãnh đạo duyệt"
                : element.classify == 17
                ? "Hoàn thành"
                : "Xác nhận hoàn thành";
          }
          element.device_approved_li = JSON.parse(element.device_approved_li);
             if(element.device_approved_li)
          element.device_approved_li.forEach((otem) => {
            if (otem.avatar == "") {
              otem.avatar = null;
            }
          });

          // if (props.checkApp == 1 && element.classify != 17) {
            if (props.checkApp == 1  && element.status==true) {
            datalists.value.push({
              name: element.approved_group_name,
              code: element.approved_group_id,
              data: element,
              users: element.device_approved_li,
            });
          }
          if (props.checkApp == 2  && element.status==true) {
            datalists.value.push({
              name: element.approved_group_name,
              code: element.approved_group_id,
              data: element,
              users: element.device_approved_li,
            });
          }
        });
        if (props.isdefault) {
          configAprroved.value.approved_group_id_fake = datalists.value.find(
            (x) => x.code === props.approved_group_id
          );
        }
      }

      options.value.loading = false;
    })
    .catch((error) => {
      console.log(error);
      options.value.loading = false;
      toast.error("Tải dữ liệu không thành công!");
    });
};
const isRefAprroved = ref();
const configAprroved = ref({
  list_user_follows: null,
  approved_group_id_fake: null,
  approved_group_id: null,
  date_send: new Date(),
});
const listDropdownUser = ref();
const loadUser = () => {
  listDropdownUser.value = [];
  axios
  .post(
      baseURL + "/api/device_card/getData",
      {
        str: encr(
          JSON.stringify({
        proc: "sys_users_list_dd",
        par: [
          { par: "search", va: null },
          { par: "user_id", va: store.getters.user.user_id },
          { par: "role_id", va: null },
          { par: "organization_id", va: store.getters.user.organization_id },
          { par: "department_id", va: null },
          { par: "position_id", va: null },
          { par: "pageno", va: 1 },
          { par: "pagesize", va: 100000 },
          { par: "isadmin", va: null },
          { par: "status", va: null },
          { par: "start_date", va: null },
          { par: "end_date", va: null },
        ],
      }),
            SecretKey,
            cryoptojs
          ).toString(),
        },config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];
      data.forEach((element, i) => {
        listDropdownUser.value.push({
          name: element.full_name,
          code: element.user_id,
          avatar: element.avatar,
          department_name: element.department_name,
          role_name: element.role_name,
        });
      });
    })
    .catch((error) => {
      console.log(error);
      toast.error("Tải dữ liệu không thành công!");
      options.value.loading = false;

      if (error && error.status === 401) {
        swal.fire({
          text: "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
      }
    });
};
const listFilesS = ref([]);
const hideAccept = () => {
  emitter.emit("emitData", { type: "hideAccept", data: null });
};
const sendAccept = () => {
  if (!configAprroved.value.approved_group_id_fake) {
    swal.fire({
      title: "Thông báo!",
      text: "Vui lòng chọn nhóm duyệt!",
      icon: "error",
      confirmButtonText: "OK",
    });
    return;
  }
  configAprroved.value.approved_group_id =
    configAprroved.value.approved_group_id_fake.code;

  let formData = new FormData();
  for (var i = 0; i < files.value.length; i++) {
    let file = files.value[i];
    formData.append("image", file);
  }
  configAprroved.value.device_process_type = props.device_process_type;
  configAprroved.value.device_process_code = props.device_process_code;
  configAprroved.value.device_note_id = props.device_note_id;

  let liUserFollows = [];
  if (list_user_follows.value.length > 0)
    list_user_follows.value.forEach((element) => {
      liUserFollows.push({
        user_id: element,
        user_fllows_type: props.device_process_type,
      });
    });

  swal
    .fire({
      title: "Thông báo",
      text: "Xác nhận trình duyệt ?",
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
        if (props.checkApp == 2) {
          if (props.listAssetsH) {
            props.listAssetsH.forEach((element) => {
              element.repair_condition = element.repair_condition_fake;
            });
          }
          configAprroved.value.device_process_id = props.device_process_id;
          formData.append("process", JSON.stringify(configAprroved.value));
          formData.append("userfollows", JSON.stringify(liUserFollows));
          formData.append("filesize", JSON.stringify(fileSize));
          formData.append("processfiles", JSON.stringify(listFilesS.value));
          formData.append(
            "details",
            configAprroved.value.device_process_type == 1
              ? JSON.stringify(props.listAssetsH)
              : null
          );
          axios
            .put(
              baseURL + "/api/device_process/accept_device_process",
              formData,
              config
            )
            .then((response) => {
              if (response.data.err != "1") {
                emitter.emit("emitData", { type: "sendAccept_1", data: null });

                swal.close();
              } else {
                swal.fire({
                  title: "Error!",
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
        } else {
          formData.append("process", JSON.stringify(configAprroved.value));
          formData.append("userfollows", JSON.stringify(liUserFollows));
          formData.append("filesize", JSON.stringify(fileSize));
          formData.append("processfiles", JSON.stringify(listFilesS.value));
          axios
            .post(baseURL + "/api/device_process/add_process", formData, config)
            .then((response) => {
              if (response.data.err != "1") {
                swal.close();
                toast.success("Trình duyệt thành công!");
                if (props.checkApp == 1) {
                  emitter.emit("emitData", { type: "sendAccept", data: null });
                }
              } else {
                swal.fire({
                  title: "Error!",
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
        }
      }
    });
};

const list_user_follows = ref([]);
onMounted(() => {
  loadData();
  loadUser();
  return { configAprroved };
});
</script><template>
  <div>
    <div class="grid">
      <div class="col-12 pb-0 field flex align-items-center">
        <div class="col-3 p-0">Nhóm duyệt</div>
        <div class="col-9 p-0">
          <div class="col-12 md:col-12 p-0 flex">
            <Dropdown
              v-model="configAprroved.approved_group_id_fake"
              :options="datalists"
              optionLabel="name"
              :filter="true"
              panelClass="d-design-dropdown"
              placeholder="Chọn nhóm duyệt"
              class=" p-0 p-design-dropdown"
              ref="isRefAprroved"
              :class="props.isdefault ? 'col-12': 'col-11'"
              :disabled="props.isdefault ? true : false"
            >
              <template #value="slotProps">
                <div
                  class="p-dropdown-car-value format-center h-full"
                  v-if="slotProps.value"
                >
                  <div>
                    <div class="font-bold p-2 px-0">
                      {{ slotProps.value.name }}
                    </div>

                    <div class="flex px-2 format-center py-0">
                      <div
                        v-for="(item, index) in slotProps.value.users"
                        :key="index"
                      >
                        <Avatar
                          v-bind:label="
                            item.avatar
                              ? ''
                              : item.name_approved.substring(
                                  item.name_approved.lastIndexOf(' ') + 1,
                                  item.name_approved.lastIndexOf(' ') + 2
                                )
                          "
                          :image="basedomainURL + item.avatar"
                          size="large"
                          :style="
                            item.avatar
                              ? 'background-color: #2196f3'
                              : 'background:' +
                                bgColor[item.name_approved.length % 7]
                          "
                          shape="circle"
                          @error="
                            $event.target.src =
                              basedomainURL + '/Portals/Image/nouser1.png'
                          "
                          class="
                            mr-2
                            border-1 border-solid border-primary
                            w-2rem
                            h-2rem
                          "
                          v-tooltip.right="{
                            value:
                              item.name_approved  ,
                            class: 'custom-error-tl',
                          }"
                        />
                      </div>
                    </div>
                  </div>
                </div>
                <div v-else class="format-center h-full">
                  {{ slotProps.placeholder }}
                </div>
              </template>
              <template #option="slotProps">
                <div class="p-dropdown-car-option format-center">
                  <div>
                    <div class="font-bold p-2 px-0">
                      {{ slotProps.option.name }}
                    </div>

                    <div class="flex px-2 py-0 format-center">
                      <div
                        v-for="(item, index) in slotProps.option.users"
                        :key="index"
                      >
                        <Avatar
                          v-bind:label="
                            item.avatar
                              ? ''
                              : item.name_approved.substring(
                                  item.name_approved.lastIndexOf(' ') + 1,
                                  item.name_approved.lastIndexOf(' ') + 2
                                )
                          "
                          :image="basedomainURL + item.avatar"
                          size="large"
                          :style="
                            item.avatar
                              ? 'background-color: #2196f3'
                              : 'background:' +
                                bgColor[item.name_approved.length % 7]
                          "
                          shape="circle"
                          @error="
                            $event.target.src =
                              basedomainURL + '/Portals/Image/nouser1.png'
                          "
                          class="
                            mr-2
                            border-1 border-solid border-primary
                            w-2rem
                            h-2rem
                          "
                          v-tooltip.right="{
                            value:
                              item.name_approved  ,
                            class: 'custom-error-tl',
                          }"
                        />
                      </div>
                    </div>
                  </div>
                </div>
              </template>
            </Dropdown>
            <div
              @click="toggleApproved"
              aria-haspopup="true"
              aria-controls="overlay_panelS"
              style="border-radius: 0px 5px 5px 0px"
              class="col-1 format-center bg-blue-500 cursor-pointer"
              v-if="props.isdefault? false : true"
            >
              <i class="pi pi-filter"></i>
            </div>
          </div>
        </div>
      </div>

      <div class="col-12 pb-0 field flex align-items-center">
        <div class="col-3 p-0">Thời gian</div>
        <div class="col-9 p-0">
          <Calendar
            class="w-full"
            v-model="configAprroved.date_send"
            autocomplete="on"
            :showIcon="false"
          />
        </div>
      </div>
      <div class="col-12 pb-0 field flex align-items-center">
        <div class="col-3 p-0">Người theo dõi</div>
        <div class="col-9 p-0">
          <MultiSelect
            v-model="list_user_follows"
            panelClass="d-design-dropdown"
            class="sel-placeholder w-full"
            :options="listDropdownUser"
            :filter="true"
            optionLabel="name"
            optionValue="code"
            placeholder="Chọn người theo dõi"
          >
            <template #option="slotProps">
              <div class="country-item flex align-items-center">
                <div class="grid w-full p-0">
                  <div
                    class="
                      field
                      p-0
                      py-1
                      col-12
                      flex
                      m-0
                      cursor-pointer
                      align-items-center
                    "
                  >
                    <div class="col-2 mx-2 p-0 align-items-center">
                      <Avatar
                        v-bind:label="
                          slotProps.option.avatar
                            ? ''
                            : slotProps.option.name.substring(
                                slotProps.option.name.lastIndexOf(' ') + 1,
                                slotProps.option.name.lastIndexOf(' ') + 2
                              )
                        "
                        :image="basedomainURL + slotProps.option.avatar"
                        size="small"
                        :style="
                          slotProps.option.avatar
                            ? 'background-color: #2196f3'
                            : 'background:' +
                              bgColor[slotProps.option.name.length % 7]
                        "
                        shape="circle"
                        @error="
                          $event.target.src =
                            basedomainURL + '/Portals/Image/nouser1.png'
                        "
                      />
                    </div>
                    <div class="col-10 p-0 pl-2 align-items-center">
                      <div class="pt-2">
                        <div class="font-bold">
                          {{ slotProps.option.name }}
                        </div>
                        <div class="flex w-full text-sm font-italic text-500">
                          <div>{{ slotProps.option.role_name }}</div>
                        </div>
                        <!-- <div
                              class="flex w-full text-sm font-italic text-500"
                            >
                              {{ slotProps.option.department_name }}
                            </div> -->
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </template>
          </MultiSelect>
        </div>
      </div>
      <div class="col-12 pb-0 field flex align-items-center">
        <div class="col-3 p-0">Nội dung</div>
        <div class="col-9 p-0">
          <Textarea
            class="w-full"
            v-model="configAprroved.content"
            rows="3"
            cols="30"
          />
        </div>
      </div>
      <div class="col-12 field flex align-items-center">
        <div class="col-3 p-0">File đính kèm</div>
        <div class="col-9 p-0">
          <FileUpload
            chooseLabel="Chọn File"
            :showUploadButton="false"
            :showCancelButton="false"
            :multiple="true"
            :maxFileSize="524288000"
            @select="onUploadFile"
            @remove="removeFile"
            :invalidFileSizeMessage="'{0}: Dung lượng File không được lớn hơn {1}'"
          >
            <template #empty>
              <p class="p-0 m-0 text-500">Kéo thả hoặc chọn File.</p>
            </template>
          </FileUpload>
        </div>
      </div>
      <div class="col-12 flex align-items-center">
        <Toolbar class="w-full surface-0 p-0 border-0 pb-0">
          <template #end>
            <div>
              <Button
                @click="hideAccept()"
                label="Hủy"
                icon="pi pi-times"
                class="mr-2 p-button-outlined"
              />
              <Button
                @click="sendAccept()"
                label="Trình duyệt"
                icon="pi pi-check"
                autofocus
              />
            </div>
          </template>
        </Toolbar>
      </div>
    </div>
  </div>
  <OverlayPanel
    ref="filterApproved"
    appendTo="body"
    :showCloseIcon="false"
    id="filterApproved"
    style="width: 400px"
    :breakpoints="{ '960px': '20vw' }"
  >
    <div class="grid formgrid m-2">
      <div class="field col-12 md:col-12 flex">
        <div class="col-4 p-0 align-items-center flex">Loại cấu hình:</div>
        <Dropdown
          v-model="filterSCard"
          :options="listSCard"
          optionLabel="name"
          :showClear="true"
          optionValue="code"
          placeholder="Chọn loại cấu hình"
          panelClass="d-design-dropdown"
          class="col-8 p-0"
        />
      </div>
      <div class="field col-12 md:col-12 flex">
        <div class="col-4 p-0 align-items-center flex">Loại duyệt:</div>
        <Dropdown
          v-model="filterTCard"
          panelClass="d-design-dropdown"
          :options="listTCard"
          :filter="true"
          optionLabel="name"
          optionValue="code"
          :showClear="true"
          class="col-8 p-0"
          placeholder="Chọn loại duyệt"
        >
        </Dropdown>
      </div>

      <div class="col-12 field p-0">
        <Toolbar class="toolbar-filter surface-0 border-0 pb-0">
          <template #start>
            <Button
              @click="reFilterCard"
              class="p-button-outlined"
              label="Xóa"
            ></Button>
          </template>
          <template #end>
            <Button @click="filterCard()" label="Lọc"></Button>
          </template>
        </Toolbar>
      </div>
    </div>
  </OverlayPanel>
</template>
<style scoped>
.custom-error-tl {
  min-width: 500px !important;
  width: 400px !important;
}
</style>