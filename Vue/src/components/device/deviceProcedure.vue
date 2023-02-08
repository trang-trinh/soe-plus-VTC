<script setup>
import { ref, inject, onMounted, watch } from "vue";
import { useToast } from "vue-toastification";
import moment from "moment";
const basedomainURL = baseURL;
const emitter = inject("emitter");
const axios = inject("axios");
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
  dataProcess: Object,
  type: String,
  device_process_code: String,
  approved_group_id: Intl,
  isdefault: Boolean,
  device_note_id: Intl,
  devicelogs:Object
});
const isThumnFiles = ref(false);
const listThumFiles = ref();
const showListFiles = (value) => {
  let arrFile = [];
  value.forEach((element) => {
    let allowedExtensions = /(\.jpg|\.jpeg|\.png|\.gif)$/i; //các tập tin cho phép

    if (allowedExtensions.exec(element.process_files_name)) {
      // Kiểm tra định dạng
      arrFile.push({
        data: element.process_files_name,
        src: baseURL + element.file_path,
        checkimg: true,
        allsrc: element.file_path,
      });
      URL.revokeObjectURL(element.file_path);
    } else {
      arrFile.push({
        data: element.process_files_name,
        src: baseURL + element.file_path,
        checkimg: false,
        allsrc: element.file_path,
      });
    }
  });
  listThumFiles.value = arrFile;
  isThumnFiles.value = true;
};
const collapsedPanel = ref(false);
const showCollapse = () => {
  if (collapsedPanel.value == true) collapsedPanel.value = false;
  else collapsedPanel.value = true;
};
const toast = useToast();
</script>

<template>
  <form>
    <div class="grid formgrid m-2">
      <div class="col-12 md:col-12 p-0">
        <TabView class="tabview-custom" ref="tabview">
          <TabPanel>
            <template #header>
              <i class="pi pi-fw pi-chart-bar"></i>
              <h3 class="m-0 mx-2">Quy trình xử lý</h3>
            </template>
            <div class="w-full">
              <Toolbar class="w-full surface-0 border-0 px-0 pt-0">
                <template #end>
                  <div>
                    <Button
                      :icon="
                        collapsedPanel == false ? 'pi pi-minus' : 'pi pi-plus'
                      "
                      @click="showCollapse"
                    ></Button>
                  </div>
                </template>
              </Toolbar>
            </div>
            
            <div v-if="props.dataProcess.length > 0">
              <div v-if="props.dataProcess[1].length > 0">
                <div v-for="(item, index) in props.dataProcess[1]" :key="index">
                  <div   class="bg-blue-200 ">
                    <Panel
                      header="Header"
                      :toggleable="true"
                      :collapsed="collapsedPanel"
                    >
                      <template #header>
                        <div class="flex">
                          <h3
                            class="m-0 format-flex-center"
                            style="text-align: left"
                          >
                            {{ item.approved_group_name?item.approved_group_name:'Người trình duyệt' }}
                          </h3>
                        <Tag
                          v-if="item.is_approved_by_department ==false"
                            class="ml-3 px-3 py-1"
                            :value="
                              item.approved_type == 1
                                ? 'Duyệt một trong nhiều'
                                : item.approved_type == 2
                                ? 'Duyệt tuần tự'
                                : item.approved_type == 3? 'Duyệt ngẫu nhiên':''
                            "
                            :class="'type' + item.approved_type"
                            style="
                              font-size: 11px;
                              min-width: max-content;
                              color: #fff;
                              border-radius: 25px;
                              height: max-content;
                            "
                          ></Tag>
                          <Tag
                         v-else-if="item.is_approved_by_department ==true"
                            class="ml-3 px-3 py-1"
                            :value="
                              'Duyệt theo phòng ban'
                            "
                            :class="'type' + item.approved_type"
                            style="
                              font-size: 11px;
                              min-width: max-content;
                              color: #fff;
                              border-radius: 25px;
                              height: max-content;
                            "
                          ></Tag>
                        </div>
                      </template>

                      <div v-if="item.listprocess.length > 0">
                        <div
                          v-for="(process, sindex) in item.listprocess"
                          :key="sindex"
                          class="flex  mb-3"
                          :class="'is-sign' + process.is_approved"
                        >
                          <div class="flex"></div>

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
                                    process.avatar
                                      ? ''
                                      : process.full_name.substring(
                                          process.full_name.lastIndexOf(' ') +
                                            1,
                                          process.full_name.lastIndexOf(' ') + 2
                                        )
                                  "
                                  v-bind:image="basedomainURL + process.avatar"
                                  v-tooltip.top="
                                    process.full_name +
                                    '<br>' +
                                    process.position_name +
                                    '<br>' +
                                    process.organization_name
                                  "
                                  style="
                                    background-color: #2196f3;
                                    color: #ffffff;
                                    width: 4rem;
                                    height: 4rem;
                                
                                  "
                                  :style="{
                                    background: bgColor[sindex % 7],
                                  }"
                                  @error="
                                    $event.target.src =
                                      basedomainURL +
                                      '/Portals/Image/nouser1.png'
                                  "
                                  class="text-avatar p-avatar-d-design"
                                  size="xlarge"
                                  shape="circle"
                                />
                                <span
                                  class="is-sign"
                                  v-if="
                                    process.is_approved !== '0' ||
                                    process.is_returned === '1'
                                  "
                                >
            
                                  <font-awesome-icon
                                    v-if="process.is_approved === '2' && process.is_shows == true"
                                    icon="fa-solid fa-circle-check"
                                    style="
                                      font-size: 16px;
                                      display: block;
                                      color: #7abd1a;
                                    "
                                  />
                                  <font-awesome-icon
                                    v-if="
                                      process.is_returned === '1' &&
                                      process.is_type === '1'  && process.is_shows == true
                                    "
                                    icon="fa-solid fa-circle-xmark"
                                    style="
                                      font-size: 16px;
                                      display: block;
                                      color: red;
                                    "
                                  />

                                  <font-awesome-icon
                                    v-if="
                                      process.is_returned === '1' &&
                                      process.is_type === '0' && process.is_shows == true
                                    "
                                    icon="fa-solid fa-circle-up"
                                    style="
                                      font-size: 16px;
                                      display: block;
                                      color: #ff8b4e;
                                    "
                                  />
                                </span>
                              </div>
                            </div>

                            <span v-if="process.date_approved != null" >
                              <span
                                class="sign-date"
                                v-if="process.date_approved != ' '"
                                >{{ process.date_approved }}
                              </span></span
                            >
                        
                          </div>
                          <div
                            class="signuser-detail"
                            :class="{
                              'signuser-last':
                                sindex === item.listprocess.length - 1,
                            }"
                          >
                            <div>
                              <h3 class="m-0 mb-2" :class="    process.is_shows == true?'':'text-900'">
                                {{ process.full_name }}
                              </h3>
                              <div class="description">
                                <div>{{ process.position_name }}</div>
                                <div>{{ process.department_name }}</div>
                              </div>
                                  
                              <div
                                class="mt-2"
                                v-if="process.is_approved === '2' &&  process.is_shows "
                              >
                                <span>Nội dung: </span>

                                <span>{{ process.content }}</span>
                              </div>
                              <div
                                class="mt-2"
                                v-if="process.is_returned === '1' &&  process.is_shows "
                              >
                                <span>Nội dung trả lại: </span>

                                <span>{{ process.content }}</span>
                              </div>

                              <div
                                v-if="
                                  process.files != '' && process.files != null
                                "
                                class="mt-2 description"
                              >
                                <div @click="showListFiles(process.files)">
                                  <span class="hover"
                                    >Tài liệu đính kèm
                                    <i class="pi pi-paperclip"></i
                                  ></span>
                                </div>
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
                    </Panel>
            
                    <Toolbar v-if="index < props.dataProcess[1].length -1" class="w-full surface-0 border-0 py-0">
                      <template #start>
                        <div>
                          <Button
                            icon="pi pi-arrow-down"
                            class="p-button-text py-0"
                          ></Button>
                          <!-- <div class="w-1rem bg-green-600"><span style="color:transparent">s</span></div> -->
                        </div>
                      </template>
                    </Toolbar>
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
          </TabPanel>
          <TabPanel>
              <template #header>
                <i class="pi pi-fw pi-history"></i>
                <h3 class="m-0 mx-2">lịch sử</h3>
              </template>
             
              <div v-if="props.devicelogs.length > 0">
                <div
                  v-for="(log, lindex) in props.devicelogs"
                  
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
                          v-tooltip.top="
                            log.full_name +
                            '<br>' +
                            log.role_name +
                            '<br>' +
                            log.department_name
                          "
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
                        > {{log.created_time}} {{ log.created_date }}
                      </span>
                    </div>
                  </div>
                  <div class="log-detail">
                    <div>
                      <h3 class="m-0 mb-2">
                        {{ log.full_name }}
                      </h3>
                      <div class="description">
                        <div>{{ log.role_name }}</div>
                        <div>{{ log.department_name }}</div>
                      </div>
                      <div class="mt-2" v-if="log.content != null">
                        <span>{{ log.content }}</span>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
              <div
                v-else
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
  <Dialog
    v-model:visible="isThumnFiles"
    :dismissableMask="true"
    :modal="true"
    :style="{ width: '40vw' }"
    header="Danh sách tệp đính kèm"
  >
   
    <div class="flex format-center">
      <div
        v-for="(element1, index1) in listThumFiles"
        :key="index1"
        class="mr-2"
      >
        <Image
          v-if="element1.checkimg"
          :src="element1.src"
          :alt="element1.data"
          width="100"
          height="100"
          style="object-fit: contain; border: 1px solid #ccc"
          preview
          @error="
            $event.target.src = basedomainURL + '/Portals/Image/noimg.jpg'
          "
        />
        <div v-else>
          <a
            v-if="element1.src != null && element1.src != ''"
            :href="element1.src"
            download
            class="w-full no-underline"
          >
            <img
              :src="
                basedomainURL +
                '/Portals/Image/file/' +
                element1.data.substring(element1.data.indexOf('.') + 1) +
                '.png'
              "
              style="
                width: 100px;
                height: 100px;

                object-fit: contain;
              "
              :alt="element1.data"
            />
            <div>
              {{ element1.data }}
            </div></a
          >
        </div>
      </div>
    </div>
  </Dialog>
</template>
<style scoped>
@import url(../../views/calendar/component/stylecalendar.css);

@media screen and (max-width: 960px) {
  ::v-deep(.customized-timeline) {
    .p-timeline-event:nth-child(even) {
      flex-direction: row !important;

      .p-timeline-event-content {
        text-align: left !important;
      }
    }

    .p-timeline-event-opposite {
      flex: 0;
    }

    .p-card {
      margin-top: 1rem;
    }
  }
}
</style>>