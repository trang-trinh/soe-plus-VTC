<script setup>
import { ref, inject, onMounted } from "vue";
import { encr } from "../../../../util/function";
import { socketMethod } from "../../../../util/methodSocket";
import moment from "moment";
import { useToast } from "vue-toastification";
import { required } from "@vuelidate/validators";
import { useVuelidate } from "@vuelidate/core";
import { de } from "date-fns/locale";
const cryoptojs = inject("cryptojs");
const store = inject("store");
const swal = inject("$swal");
const axios = inject("axios");
const emitter = inject("emitter");

const config = {
  headers: {
    Authorization: `Bearer ${store.getters.token}`,
  },
};
const basedomainURL = baseURL;
const bgColor = ref([
  "#F8E69A",
  "#AFDFCF",
  "#F4B2A3",
  "#9A97EC",
  "#CAE2B0",
  "#8BCFFB",
  "#CCADD7",
]);
//Get arguments
const props = defineProps({
  headerDialog: String,
  displayDialog: Boolean,
  closeDialog: Function,
  modelsend: Object,
});

//Function
const goFile = (file) => {
  window.open(basedomainURL + file.file_path, "_blank");
};
const displayBasic = ref(false);
//Thêm bản ghi
const type_process = ref(0);
const datalists = ref([]);
const loadData = () => {
  axios
    .post(
      baseURL + "/api/hrm_ca_SQL/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "hrm_process_chart",
            par: [
              {
                par: "key_id",
                va: props.modelsend.key_id,
              },
              {
                par: "type_module",
                va: props.modelsend.type_module,
              },
            ],
          }),
          SecretKey,
          cryoptojs
        ).toString(),
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];
      let dataProcess = JSON.parse(response.data.data)[1];
      if (data) {
        
        data.forEach((element) => {
          if(element.approved_type==2){
            element.type_name="Duyệt tuần tự";
        
         }
         else     if(element.approved_type==1){
          element.type_name="Duyệt một trong nhiều";

         }
         else  if(element.approved_type==3){
          element.type_name="Duyệt ngẫu nhiên";
         }
      
         else
         element.type_name="Không xác định";
          if (element.hrm_process) {
            element.hrm_process = JSON.parse(element.hrm_process);
            element.hrm_process.forEach(item => {
              
              if(item.files)
              item.files = JSON.parse(item.files);
            });
          }
        });
      }
      if (dataProcess) {
        dataProcess.forEach((item) => {
         if(item.config_process_type == 2){
          item.type_name="Duyệt tuần tự";
        
         }
         else     if(item.config_process_type==1)
         {
          item.type_name="Duyệt một trong nhiều";

         }
         else  if(item.config_process_type==3){
          item.type_name="Duyệt ngẫu nhiên";
         }
         else
         item.type_name="Không xác định";
          item.aprroves_groups = data.filter(
            (x) => x.config_process_form_id == item.config_process_form_id
          );
        });

        datalists.value = dataProcess;
      }
       
    })
    .catch((error) => {
      console.log(error);
    });
};
onMounted(() => {
  loadData();
  displayBasic.value = props.displayDialog;
  return {};
});
</script>
<template>
  <Dialog
    :header="props.headerDialog"
    v-model:visible="displayBasic"
    :style="{ width: '40vw' }"
    :maximizable="false"
    :closable="true"
    :modal="true"
    style="z-index: 1002"
    @hide="props.closeDialog"
  >
    <form>
      <div class="grid formgrid m-2">
        <div class="col-12 md:col-12 p-0">
          <TabView class="tabview-custom" ref="tabview">
            <TabPanel>
              <template #header>
                <i class="pi pi-fw pi-chart-bar"></i>
                <h3 class="m-0 mx-2">Quy trình xử lý</h3>
              </template>
              <div  >
            
                <div v-if="datalists.length > 0">
                  <div
                    v-for="(item, pindex) in datalists"
                    :class="{ 'is-close': item.is_close }"
                    :key="pindex"
                    class="bg-blue-200 mb-3"
                  >
                  <div  > <Panel
                      header="Header"
                      :toggleable="true"
                      :collapsed="item.is_close"
                    >
                      <template #header>
                        <div class="flex">
                          <h3
                            class="m-0 format-flex-center"
                            style="text-align: left"
                          >
                           {{item.config_process_name}}
                          </h3>
                          <Tag v-if="item.type_send!=2"
                            class="ml-3 px-3 py-1"
                            :value="item.type_name"
                            :class="'type' + item.config_process_type"
                            style="
                              font-size: 11px;
                              min-width: max-content;
                              color: #fff;
                              border-radius: 25px;
                              height: max-content;
                            "
                          ></Tag>
                          <Tag
                            v-if="item.is_close"
                            class="mx-3 px-3 py-1"
                            :value="'Đã hủy'"
                            :style="{
                              backgroundColor: 'red',
                              color: '#fff',
                            }"
                            style="
                              font-size: 11px;
                              min-width: max-content;
                              height: max-content;
                            "
                          ></Tag>
                        </div>
                      </template>
                      <div v-if="item.aprroves_groups.length > 0">
                        <div class=" mt-3"
                          v-for="(sign, sindex) in item.aprroves_groups"
                          :key="sindex"
                        >
                          <div
                            class="p-2 flex"
                            style="
                              background-color: antiquewhite;
                              justify-content: space-between;
                            "
                          >
                            <h3 class="m-0 format-flex-center">
                              {{ sign.approved_group_name }} ({{
                                sign.hrm_process.length
                              }}
                              người)
                            </h3>
                            <Tag
                              class="ml-3 px-3 py-1"
                              :value="sign.type_name"
                              :class="'type' + sign.approved_type"
                              style="
                                font-size: 11px;
                                min-width: max-content;
                                color: #fff;
                                border-radius: 25px;
                                height: max-content;
                              "
                            ></Tag>
                          </div>
                          <div
                           
                            v-if="sign.hrm_process.length > 0"
                          >
                            <div 
                              v-for="(signuser, uindex) in sign.hrm_process"
                              :key="uindex"
                              class="flex mt-3"
                              :class="
                                'is-sign' +
                                signuser.is_approved +
                                ' status-sign' +
                                signuser.is_returned
                              "
                            >
                              <div class="signuser-image">
                                <div class="group-sign">
                                  <div
                                    style="
                                      display: inline-block;
                                      position: relative;
                                      z-index: 1;
                                    "
                                  >
                                    <Avatar
                                      v-bind:label="
                                        signuser.avatar
                                          ? ''
                                          : signuser.last_name.substring(0, 1)
                                      "
                                      v-bind:image="
                                        basedomainURL + signuser.avatar
                                      "
                                      v-tooltip.top="signuser.full_name"
                                      style="
                                        background-color: #2196f3;
                                        color: #ffffff;
                                        width: 4rem;
                                        height: 4rem;
                                      "
                                      :style="{
                                        background: bgColor[sindex % 7],
                                      }"
                                      class="text-avatar"
                                      size="xlarge"
                                      shape="circle"
                                    />
                                    <span
                                      class="is-sign"
                                     
                                    >
                                    
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
                                      <!-- <font-awesome-icon
                                        v-if="signuser.is_sign === -3"
                                        icon="fa-solid fa-circle-xmark"
                                        style="
                                          font-size: 16px;
                                          display: block;
                                          color: red;
                                        "
                                      /> -->
                                    </span>
                                  </div>
                                </div>
                                <span
                                  class="sign-date"
                                  v-if="signuser.date_approved != null"
                                  >{{ signuser.date_approved }}
                                </span>
                              </div>
                              <div
                                class="signuser-detail"
                                :class="{
                                  'signuser-last':
                                    uindex === sign.hrm_process.length - 1,
                                }"
                              >
                                <div>
                                  <h3 class="m-0 mb-2">
                                    {{ signuser.full_name }}
                                  </h3>
                                  <div class="description">
                                    <div>{{ signuser.position_name }}</div>
                                    <div>{{ signuser.department_name }}</div>
                                  </div>
                                  <div
                                    class="mt-2"
                                    v-if="signuser.content != null"
                                  >
                                    <span v-if="sign.approved_type === 0"
                                      >Trình duyệt:
                                    </span>
                                    <span
                                      v-else-if="
                                        signuser.is_approved === '1'  
                                      "
                                      >Chấp thuận:
                                    </span>
                                    <span
                                      v-else-if="
                                        signuser.is_returned === '1'  
                                      "
                                      >Trả lại:
                                    </span>
                                    <!-- <span
                                      v-if="
                                        signuser.is_type === 0 &&
                                        signuser.is_sign == -3
                                      "
                                      >Hủy lịch:
                                    </span> -->
                                    <span>{{ signuser.content }}</span>
                                  </div>
                                  <div
                                    v-if="
                                      signuser.files &&
                                      signuser.files.length > 0
                                    "
                                    class="mt-2 description"
                                  >
                                    <a
                                      class="hover"
                                      @click="goFile(signuser.files[0])"
                                      >Tài liệu đính kèm
                                      <i class="pi pi-paperclip"></i
                                    ></a>
                                  </div>
                                </div>
                              </div>
                            </div>
                          </div>
                          <div
                            v-else
                            style="width: 100%; height: 100px"
                            class="format-flex-center"
                          >
                            <span class="description"
                              >Không có người duyệt</span
                            >
                          </div>
                        </div>
                      </div>
                      <div
                        v-else
                        style="width: 100%; height: 100px"
                        class="format-flex-center"
                      >
                        <span class="description">Không có nhóm duyệt</span>
                      </div>
                    </Panel>
                    </div>
                  </div>
                </div>
                <div
                  v-else
                  class="w-full format-flex-center"
                  style="height: 100px"
                >
                  <span class="description">Chưa có quy trình xử lý</span>
                </div>
              </div>
              <!-- <div v-else-if="props.is_type_calendar === 1">
                <div v-if="props.chartsigns.length > 0">
                  <div
                    v-for="(sign, sindex) in props.chartsigns"
                    :class="{ 'is-close': sign.is_close }"
                    :key="sindex"
                    class="bg-blue-200 mb-3"
                  >
                    <Panel
                      header="Header"
                      :toggleable="true"
                      :collapsed="sign.is_close"
                    >
                      <template #header>
                        <div class="flex">
                          <h3
                            class="m-0 format-flex-center"
                            style="text-align: left"
                          >
                            {{ sign.sign_name }}
                          </h3>
                          <Tag
                            class="ml-3 px-3 py-1"
                            :value="sign.type_name"
                            :class="'type' + sign.is_type"
                            style="
                              font-size: 11px;
                              min-width: max-content;
                              color: #fff;
                              border-radius: 25px;
                              height: max-content;
                            "
                          ></Tag>
                          <Tag
                            v-if="sign.is_close"
                            class="mx-3 px-3 py-1"
                            :value="'Đã hủy'"
                            :style="{
                              backgroundColor: 'red',
                              color: '#fff',
                            }"
                            style="
                              font-size: 11px;
                              min-width: max-content;
                              height: max-content;
                            "
                          ></Tag>
                        </div>
                      </template>
                      <div v-if="sign.chartsignusers.length > 0">
                        <div v-if="sign.chartsignusers.length > 0">
                          <div
                            v-for="(signuser, uindex) in sign.chartsignusers"
                            :key="uindex"
                            class="flex"
                            :class="
                              'is-sign' +
                              signuser.is_sign +
                              ' status-sign' +
                              signuser.status
                            "
                          >
                            <div class="signuser-image">
                              <div class="group-sign">
                                <div
                                  style="
                                    display: inline-block;
                                    position: relative;
                                    z-index: 1;
                                  "
                                >
                                  <Avatar
                                    v-bind:label="
                                      signuser.avatar
                                        ? ''
                                        : signuser.last_name.substring(0, 1)
                                    "
                                    v-bind:image="
                                      basedomainURL + signuser.avatar
                                    "
                                    v-tooltip.top="signuser.full_name"
                                    style="
                                      background-color: #2196f3;
                                      color: #ffffff;
                                      width: 4rem;
                                      height: 4rem;
                                    "
                                    :style="{
                                      background: bgColor[sindex % 7],
                                    }"
                                    class="text-avatar"
                                    size="xlarge"
                                    shape="circle"
                                  />
                                  <span
                                    class="is-sign"
                                    v-if="
                                      signuser.is_sign !== 0 &&
                                      signuser.is_sign !== 1
                                    "
                                  >
                                    <font-awesome-icon
                                      v-if="signuser.is_sign === 2"
                                      icon="fa-solid fa-circle-check"
                                      style="
                                        font-size: 16px;
                                        display: block;
                                        color: #7abd1a;
                                      "
                                    />
                                    <font-awesome-icon
                                      v-if="signuser.is_sign === -1"
                                      icon="fa-solid fa-circle-stop"
                                      style="
                                        font-size: 16px;
                                        display: block;
                                        color: #ff8b4e;
                                      "
                                    />
                                    <font-awesome-icon
                                      v-if="signuser.is_sign === -3"
                                      icon="fa-solid fa-circle-xmark"
                                      style="
                                        font-size: 16px;
                                        display: block;
                                        color: red;
                                      "
                                    />
                                  </span>
                                </div>
                              </div>
                              <span
                                class="sign-date"
                                v-if="signuser.sign_date != null"
                                >{{ signuser.sign_date }}
                              </span>
                            </div>
                            <div
                              class="signuser-detail"
                              :class="{
                                'signuser-last':
                                  uindex === sign.chartsignusers.length - 1,
                              }"
                            >
                              <div>
                                <h3 class="m-0 mb-2">
                                  {{ signuser.full_name }}
                                </h3>
                                <div class="description">
                                  <div>{{ signuser.position_name }}</div>
                                  <div>{{ signuser.department_name }}</div>
                                </div>
                                <div
                                  class="mt-2"
                                  v-if="signuser.sign_content != null"
                                >
                                  <span v-if="signuser.is_type === 1"
                                    >Trình duyệt:
                                  </span>
                                  <span
                                    v-if="
                                      signuser.is_type === 0 &&
                                      signuser.is_sign == 2
                                    "
                                    >Chấp thuận:
                                  </span>
                                  <span
                                    v-if="
                                      signuser.is_type === 0 &&
                                      signuser.is_sign == -1
                                    "
                                    >Trả lại:
                                  </span>
                                  <span
                                    v-if="
                                      signuser.is_type === 0 &&
                                      signuser.is_sign == -3
                                    "
                                    >Hủy lịch:
                                  </span>
                                  <span>{{ signuser.sign_content }}</span>
                                </div>
                                <div
                                  v-if="
                                    signuser.files && signuser.files.length > 0
                                  "
                                  class="mt-2 description"
                                >
                                  <a
                                    class="hover"
                                    @click="goFile(signuser.files[0])"
                                    >Tài liệu đính kèm
                                    <i class="pi pi-paperclip"></i
                                  ></a>
                                </div>
                              </div>
                            </div>
                          </div>
                        </div>
                        <div
                          v-else
                          style="width: 100%; height: 100px"
                          class="format-flex-center"
                        >
                          <span class="description">Không có người duyệt</span>
                        </div>
                      </div>
                      <div
                        v-else
                        style="width: 100%; height: 100px"
                        class="format-flex-center"
                      >
                        <span class="description">Không có người duyệt</span>
                      </div>
                    </Panel>
                  </div>
                </div>
                <div
                  v-else
                  class="w-full format-flex-center"
                  style="height: 100px"
                >
                  <span class="description">Không có nhóm duyệt</span>
                </div>
              </div>
              <div v-else-if="props.is_type_calendar === 2">
                <div v-if="chartsignusers.length > 0">
                  <div
                    v-for="(signuser, uindex) in chartsignusers"
                    :key="uindex"
                    class="flex"
                    :class="
                      'is-sign' +
                      signuser.is_sign +
                      ' status-sign' +
                      signuser.status
                    "
                  >
                    <div class="signuser-image">
                      <div class="group-sign">
                        <div
                          style="
                            display: inline-block;
                            position: relative;
                            z-index: 1;
                          "
                        >
                          <Avatar
                            v-bind:label="
                              signuser.avatar
                                ? ''
                                : signuser.last_name.substring(0, 1)
                            "
                            v-bind:image="basedomainURL + signuser.avatar"
                            v-tooltip.top="signuser.full_name"
                            style="
                              background-color: #2196f3;
                              color: #ffffff;
                              width: 4rem;
                              height: 4rem;
                            "
                            :style="{
                              background: bgColor[sindex % 7],
                            }"
                            class="text-avatar"
                            size="xlarge"
                            shape="circle"
                          />
                          <span
                            class="is-sign"
                            v-if="
                              signuser.is_sign !== 0 && signuser.is_sign !== 1
                            "
                          >
                            <font-awesome-icon
                              v-if="signuser.is_sign === 2"
                              icon="fa-solid fa-circle-check"
                              style="
                                font-size: 16px;
                                display: block;
                                color: #7abd1a;
                              "
                            />
                            <font-awesome-icon
                              v-if="signuser.is_sign === -1"
                              icon="fa-solid fa-circle-stop"
                              style="
                                font-size: 16px;
                                display: block;
                                color: #ff8b4e;
                              "
                            />
                            <font-awesome-icon
                              v-if="signuser.is_sign === -3"
                              icon="fa-solid fa-circle-xmark"
                              style="
                                font-size: 16px;
                                display: block;
                                color: red;
                              "
                            />
                          </span>
                        </div>
                      </div>
                      <span class="sign-date" v-if="signuser.sign_date != null"
                        >{{ signuser.sign_date }}
                      </span>
                    </div>
                    <div
                      class="signuser-detail"
                      :class="{
                        'signuser-last': uindex === chartsignusers.length - 1,
                      }"
                    >
                      <div>
                        <h3 class="m-0 mb-2">
                          {{ signuser.full_name }}
                        </h3>
                        <div class="description">
                          <div>{{ signuser.position_name }}</div>
                          <div>{{ signuser.department_name }}</div>
                        </div>
                        <div class="mt-2" v-if="signuser.sign_content != null">
                          <span v-if="signuser.is_type === 1"
                            >Trình duyệt:
                          </span>
                          <span
                            v-if="
                              signuser.is_type === 0 && signuser.is_sign == 2
                            "
                            >Chấp thuận:
                          </span>
                          <span
                            v-if="
                              signuser.is_type === 0 && signuser.is_sign == -1
                            "
                            >Trả lại:
                          </span>
                          <span
                            v-if="
                              signuser.is_type === 0 && signuser.is_sign == -3
                            "
                            >Hủy lịch:
                          </span>
                          <span>{{ signuser.sign_content }}</span>
                        </div>
                        <div
                          v-if="signuser.files && signuser.files.length > 0"
                          class="mt-2 description"
                        >
                          <a class="hover" @click="goFile(signuser.files[0])"
                            >Tài liệu đính kèm <i class="pi pi-paperclip"></i
                          ></a>
                        </div>
                      </div>
                    </div>
                  </div>
                </div>
                <div
                  v-else
                  style="width: 100%; height: 100px"
                  class="format-flex-center"
                >
                  <span class="description">Không có người duyệt</span>
                </div>
              </div> -->
               
            </TabPanel>
            <TabPanel>
              <template #header>
                <i class="pi pi-fw pi-history"></i>
                <h3 class="m-0 mx-2">Lịch sử</h3>
              </template>
              <!-- <div v-if="props.chartlogs.length > 0">
                <div
                  v-for="(log, lindex) in props.chartlogs"
                  :class="'is-close' + log.is_close"
                  :key="lindex"
                  class="flex"
                >
                  <div class="log-image">
                    <div class="group-sign">
                      <div
                        style="
                          display: inline-block;
                          position: relative;
                          z-index: 1;
                        "
                      >
                        <Avatar
                          v-bind:label="
                            log.avatar ? '' : log.last_name.substring(0, 1)
                          "
                          v-bind:image="basedomainURL + log.avatar"
                          v-tooltip.top="log.full_name"
                          style="
                            background-color: #2196f3;
                            color: #ffffff;
                            width: 4rem;
                            height: 4rem;
                          "
                          :style="{
                            background: bgColor[lindex % 7],
                          }"
                          class="text-avatar"
                          size="xlarge"
                          shape="circle"
                        />
                      </div>
                      <span class="sign-date" v-if="log.created_date != null"
                        >{{ log.created_date }}
                      </span>
                    </div>
                  </div>
                  <div class="log-detail">
                    <div>
                      <h3 class="m-0 mb-2">
                        {{ log.full_name }}
                      </h3>
                      <div class="description">
                        <div>{{ log.position_name }}</div>
                        <div>{{ log.department_name }}</div>
                      </div>
                      <div class="mt-2" v-if="log.message != null">
                        <span>{{ log.message }}</span>
                      </div>
                    </div>
                  </div>
                </div>
              </div>-->
              <div
                
                class="w-full format-flex-center"
                style="height: 100px"
              >
                <span class="description">Chưa có lịch sử ghi nhận</span>
              </div> 
            </TabPanel>
          </TabView>
        </div>
      </div>
    </form>
    <template #footer>
      <Button
        label="Hủy"
        icon="pi pi-times"
        @click="props.closeDialog()"
        class="p-button-outlined"
      />
    </template>
  </Dialog>
</template>
<style scoped>
@import url(../../recruitment/component/stylecalendar.css);
</style>
<style lang="scss" scoped>
::v-deep(.d-lang-table) {
  .p-datatable-thead .justify-content-center .p-column-header-content {
    justify-content: center !important;
  }

  .p-datatable-table {
    position: absolute;
  }

  .p-datatable-thead {
    position: sticky;
    top: 0;
    z-index: 1;
  }
}
::v-deep(.form-group) {
  .p-multiselect .p-multiselect-label,
  .p-dropdown .p-dropdown-label {
    height: 100%;
    display: flex;
    align-items: center;
  }
  .p-chip img {
    margin: 0;
  }
  .p-avatar-text {
    font-size: 1rem;
  }
}
::v-deep(.avatar-item) {
  .p-avatar.p-avatar-lg {
    width: 3rem;
    height: 3rem;
  }
}
::v-deep(.is-close) {
  .p-panel-header {
    color: red;
  }
}
</style>
