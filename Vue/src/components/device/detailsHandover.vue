
<script setup>
import { ref, inject, onMounted } from "vue";

import { encr } from "../../util/function.js";
import moment from "moment";
const axios = inject("axios");
const store = inject("store");

const basedomainURL = baseURL;

const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};
const cryoptojs = inject("cryptojs");
const props = defineProps({
  handover: Object,

  check: Intl,
});
const checkImg = (src) => {
  let allowedExtensions = /(\.jpg|\.jpeg|\.png|\.gif)$/i;
  return allowedExtensions.exec(src);
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
onMounted(() => {
  axios
    .post(
      baseURL + "/api/device_card/getData",
      {
        str: encr(
          JSON.stringify({
            proc: "device_handover_get",
            par: [{ par: "handover_id", va: props.handover.handover_id }],
          }),
          SecretKey,
          cryoptojs
        ).toString(),
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];
      let data1 = JSON.parse(response.data.data)[1];
      let data2 = JSON.parse(response.data.data)[2];
      device_handover.value = data[0];

      listAssetsH.value = data1;
     
      listFilesS.value = data2;
    })
    .catch((error) => {
      console.log(error);
    });
  return {};
});
const device_handover = ref();
const listAssetsH = ref([]);
const listFilesS = ref([]);
</script>
<template>
  <form v-if="device_handover">
    <div class="grid formgrid flex">
      <Splitter style="height: 500px" class="w-full border-none">
        <SplitterPanel :size="65">
          <div class="w-full h-full">
            <!-- <div class="format-center font-bold text-2xl p-3">
                Thông tin chi tiết
              </div> -->

            <!-- <div>{{ device_handover }}</div> -->
            <div class="grid p-3 pl-5">
              <div class="field col-12 flex">
                <div class="col-6 p-0 flex">
                  <div class="col-4 p-0 font-bold text-lg">
                    Thông tin phiếu:
                  </div>
                </div>
              </div>
              <div class="col-12 field flex">
                <div class="col-6 p-0 flex">
                  <div class="col-4 p-0">Số phiếu:</div>
                  <div class="col-8 p-0 font-bold">
                    {{ device_handover.handover_number }}
                  </div>
                </div>
                <div class="col-6 p-0 flex">
                  <div class="col-4 p-0">Ngày lập:</div>
                  <div class="col-8 p-0 font-bold">
                    {{
                      moment(
                        new Date(device_handover.handover_created_date)
                      ).format("DD/MM/YYYY")
                    }}
                  </div>
                </div>
              </div>
              <div class="col-12 field flex">
                <div class="col-6 p-0 flex">
                   <div class="col-4 p-0">Loại bàn giao:</div>
                  <div class="col-8 p-0 font-bold">
                    {{
                      device_handover.device_repair_id == null
                        ? "Cấp phát mới"
                        : "Thay thế tài sản"
                    }}
                  </div>
                </div>
                <div class="col-6 p-0 flex">
                  <div class="col-4 p-0">Kiểu bàn giao:</div>
                  <div class="col-8 p-0 font-bold">
                    {{
                      device_handover.handover_type == 1
                        ? "Bàn giao 3 bên"
                        : "Bàn giao 2 bên"
                    }}
                  </div>
                </div>
              </div>
               <div class="col-12 field flex" v-if=" device_handover.device_repair_id">
                <div class="col-6 p-0 flex">
                  <div class="col-4 p-0">Số phiếu sửa chữa:</div>
                  <div class="col-8 p-0 font-bold">
                    {{ device_handover.repair_number }}
                  </div>
                </div>
                <div class="col-6 p-0 flex">
                  
                </div>
              </div>
              <div class="field py-2 col-12 flex">
                <div class="col-6 p-0 flex">
                  <div class="col-4 p-0 font-bold text-lg">Người bàn giao:</div>
                </div>
              </div>
              <div class="field col-12 flex">
                <div class="col-6 p-0 flex align-items-center">
                  <div class="col-4 p-0">Người bàn giao:</div>
                  <div class="col-8 p-0 font-bold">
                    <div class="w-full">
                      <div
                        class="flex surface-0 align-items-center pr-2"
                        style="border-radius: 16px"
                      >
                        <Avatar
                          v-bind:label="
                            device_handover.user_deliver_avatar
                              ? ''
                              : device_handover.user_deliver_name.substring(
                                  device_handover.user_deliver_name.lastIndexOf(
                                    ' '
                                  ) + 1,
                                  device_handover.user_deliver_name.lastIndexOf(
                                    ' '
                                  ) + 2
                                )
                          "
                          :image="
                            basedomainURL + device_handover.user_deliver_avatar
                          "
                          size="small"
                          :style="
                            device_handover.user_deliver_avatar
                              ? 'background-color: #2196f3'
                              : 'background:' +
                                bgColor[
                                  device_handover.user_deliver_name.length % 7
                                ]
                          "
                          shape="circle"
                          @error="
                            $event.target.src =
                              basedomainURL + '/Portals/Image/nouser1.png'
                          "
                        />
                        <div class="px-2">
                          {{ device_handover.user_deliver_name }}
                        </div>
                      </div>
                    </div>
                  </div>
                </div>
                <div class="col-6 p-0 flex">
                  <div class="col-4 p-0">Phòng ban:</div>
                  <div class="col-8 p-0 font-bold">
                    {{ device_handover.user_deliver_department_name }}
                  </div>
                </div>
              </div>
              <div class="col-12 field flex">
                <div class="col-6 p-0 flex"></div>
                <div class="col-6 p-0 flex">
                  <div class="col-4 p-0">Chức vụ:</div>
                  <div class="col-8 p-0 font-bold">
                    {{ device_handover.user_deliver_position }}
                  </div>
                </div>
              </div>
              <div class="field py-2 col-12 flex">
                <div class="col-6 p-0 flex">
                  <div class="col-4 p-0 font-bold text-lg">Người nhận:</div>
                </div>
              </div>
              <div class="field col-12 flex">
                <div class="col-6 p-0 flex align-items-center">
                  <div class="col-4 p-0">Người nhận:</div>
                  <div class="col-8 p-0 font-bold">
                    <div class="w-full">
                      <div
                        class="flex surface-0 align-items-center pr-2"
                        style="border-radius: 16px"
                      >
                        <Avatar
                          v-bind:label="
                            device_handover.user_receiver_avatar
                              ? ''
                              : device_handover.user_receiver_name.substring(
                                  device_handover.user_receiver_name.lastIndexOf(
                                    ' '
                                  ) + 1,
                                  device_handover.user_receiver_name.lastIndexOf(
                                    ' '
                                  ) + 2
                                )
                          "
                          :image="
                            basedomainURL + device_handover.user_receiver_avatar
                          "
                          size="small"
                          :style="
                            device_handover.user_receiver_avatar
                              ? 'background-color: #2196f3'
                              : 'background:' +
                                bgColor[
                                  device_handover.user_receiver_name.length % 7
                                ]
                          "
                          shape="circle"
                          @error="
                            $event.target.src =
                              basedomainURL + '/Portals/Image/nouser1.png'
                          "
                        />
                        <div class="px-2">
                          {{ device_handover.user_receiver_name }}
                        </div>
                      </div>
                    </div>
                  </div>
                </div>
                <div class="col-6 p-0 flex">
                  <div class="col-4 p-0">Phòng ban:</div>
                  <div class="col-8 p-0 font-bold">
                    {{ device_handover.user_receiver_department_name }}
                  </div>
                </div>
              </div>
              <div class="col-12 field flex">
                <div class="col-6 p-0 flex">
                  <div class="col-4 p-0"  v-if="device_handover.receiver_date">Ngày xác nhận:</div>
                  <div
                    class="col-8 p-0 font-bold"
                    v-if="device_handover.receiver_date"
                  >
                    {{
                      moment(new Date(device_handover.receiver_date)).format(
                        "DD/MM/YYYY"
                      )
                    }}
                  </div>
                </div>
                <div class="col-6 p-0 flex">
                  <div class="col-4 p-0">Chức vụ:</div>
                  <div class="col-8 p-0 font-bold">
                    {{ device_handover.user_receiver_position }}
                  </div>
                </div>
              </div>
              <div class="col-12 p-0" v-if="device_handover.handover_type == 1">
                <div class="field py-2 col-12 flex">
                  <div class="col-6 p-0 flex">
                    <div class="col-4 p-0 font-bold text-lg">
                      Người xác nhận:
                    </div>
                  </div>
                </div>
                <div class="field col-12 flex">
                  <div class="col-6 p-0 flex align-items-center">
                    <div class="col-4 p-0">Người xác nhận:</div>
                    <div class="col-8 p-0 font-bold">
                      <div class="w-full">
                        <div
                          class="flex surface-0 align-items-center pr-2"
                          style="border-radius: 16px"
                        >
                          <Avatar
                            v-bind:label="
                              device_handover.user_verifier_avatar
                                ? ''
                                : device_handover.user_verifier_name.substring(
                                    device_handover.user_verifier_name.lastIndexOf(
                                      ' '
                                    ) + 1,
                                    device_handover.user_verifier_name.lastIndexOf(
                                      ' '
                                    ) + 2
                                  )
                            "
                            :image="
                              basedomainURL +
                              device_handover.user_verifier_avatar
                            "
                            size="small"
                            :style="
                              device_handover.user_verifier_avatar
                                ? 'background-color: #2196f3'
                                : 'background:' +
                                  bgColor[
                                    device_handover.user_verifier_name.length %
                                      7
                                  ]
                            "
                            shape="circle"
                            @error="
                              $event.target.src =
                                basedomainURL + '/Portals/Image/nouser1.png'
                            "
                          />
                          <div class="px-2">
                            {{ device_handover.user_verifier_name }}
                          </div>
                        </div>
                      </div>
                    </div>
                  </div>
                  <div class="col-6 p-0 flex">
                    <div class="col-4 p-0">Phòng ban:</div>
                    <div class="col-8 p-0 font-bold">
                      {{ device_handover.user_verifier_department_name }}
                    </div>
                  </div>
                </div>
                <div class="col-12 field flex">
                  <div class="col-6 p-0 flex">
                    <div  v-if="device_handover.verifier_date" class="col-4 p-0">Ngày xác nhận:</div>
                    <div
                      class="col-8 p-0 font-bold"
                      v-if="device_handover.verifier_date"
                    >
                      {{
                        moment(new Date(device_handover.verifier_date)).format(
                          "DD/MM/YYYY"
                        )
                      }}
                    </div>
                  </div>
                  <div class="col-6 p-0 flex">
                    <div class="col-4 p-0">Chức vụ:</div>
                    <div class="col-8 p-0 font-bold">
                      {{ device_handover.user_verifier_position }}
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </SplitterPanel>
        <SplitterPanel
          class="align-items-center justify-content-center"
          :size="35"
          :minSize="30"
          v-if="props.check == 0"
        >
          <div style="height: 500px" class="overflow-scroll">
            <div class="format-center font-bold text-lg p-3">
              Danh sách thiết bị kèm theo
            </div>
            <div
              class="w-full p-2"
              v-for="(item, index) in listAssetsH"
              :key="index"
            >
              <div
                style="border-radius: 10px"
                class="
                  product-item
                  border-3 border-solid border-round-3xl border-blue-100
                  surface-50
                  p-2
                  cursor-pointer
                "
              >
                <div class="image-container pr-2">
                  <img
                    :src="
                      item.image
                        ? basedomainURL + item.image
                        : basedomainURL + '/Portals/Image/noimg.jpg'
                    "
                    style="object-fit: cover; width: 125px; height: 75px"
                  />
                </div>
                <div class="product-list-detail">
                  <h5 class="my-2 text-justify">
                    {{ item.device_name }}
                  </h5>

                  <div class="flex pb-2">
                    <div class="w-full">
                      <i
                        class="pi pi-tag product-category-icon"
                        v-tooltip.top="'Số hiệu'"
                      ></i>
                      <span class="product-category">{{
                        item.device_number
                      }}</span>
                    </div>
                    <div class="w-full">
                      <i
                        class="pi pi-qrcode product-category-icon"
                        v-tooltip.top="'Mã barcode'"
                      ></i>
                      <span class="product-category">{{
                        item.barcode_id
                      }}</span>
                    </div>
                  </div>
                  <div class="flex">
                 
                    <div class="w-full">
                      <i
                        class="pi pi-shopping-cart product-category-icon"
                        v-tooltip.top="'Ngày mua'"
                      ></i>
                      <span class="product-category">
                        {{
                          moment(new Date(item.purchase_date)).format(
                            "DD/MM/YYYY"
                          )
                        }}
                      </span>
                    </div>
                  </div>
                </div>
               
              </div>
            </div>
           
            <div
              class="format-center font-bold text-lg p-3"
              v-if="listFilesS.length > 0"
            >
              Danh sách File đính kèm
            </div>

            <div class="w-full p-2">
              <div
                class="p-0 mb-2 field w-full"
                v-for="(item, index) in listFilesS"
                :key="index"
              >
                <div
                  class="
                    p-0
                    border-3 border-solid border-round-3xl border-blue-200
                    surface-50
                  "
                  style="width: 100%; border-radius: 10px"
                >
                  <Toolbar class="w-full py-3">
                    <template #start>
                      <div class="flex align-items-center">
                        <Image
                          v-if="checkImg(item.file_path)"
                          :src="basedomainURL + item.file_path"
                          :alt="item.file_name"
                          width="70"
                          height="50"
                          style="
                            object-fit: contain;
                            border: 1px solid #ccc;
                            width: 70px;
                            height: 50px;
                          "
                          preview
                          @error="
                            $event.target.src =
                              basedomainURL + '/Portals/Image/noimg.jpg'
                          "
                          class="pr-2"
                        />
                        <div v-else>
                          <a
                            :href="basedomainURL + item.file_path"
                            download
                            class="w-full no-underline"
                          >
                            <img
                              :src="
                                basedomainURL +
                                '/Portals/Image/file/' +
                                item.file_path.substring(
                                  item.file_path.lastIndexOf('.') + 1
                                ) +
                                '.png'
                              "
                              style="
                                width: 70px;
                                height: 50px;
                                object-fit: contain;
                              "
                              :alt="item.file_name"
                            />
                          </a>
                        </div>
                        <div class="align-items-center h-full">
                          <div>
                            <a
                              :href="basedomainURL + item.file_path"
                              download
                              class="
                                w-full
                                no-underline
                                text-900
                                align-items-center
                              "
                            >
                              <span
                                class="ml-2 text-900"
                                style="line-height: 50px"
                              >
                                {{ item.file_name }}</span
                              >
                            </a>
                          </div>
                        </div>
                      </div>
                    </template>
                    <template #end> </template>
                  </Toolbar>
                </div>
              </div>
            </div>
          </div>
        </SplitterPanel>
      </Splitter>

      <div class="w-full" v-if="props.check == 1">
        <div
          class="format-center font-bold text-lg p-3"
          v-if="listFilesS.length > 0"
        >
          Danh sách File đính kèm
        </div>

        <div class="w-full p-2">
          <div
            class="p-0 mb-2 field w-full"
            v-for="(item, index) in listFilesS"
            :key="index"
          >
            <div
              class="
                p-0
                border-3 border-solid border-round-3xl border-blue-200
                surface-50
              "
              style="width: 100%; border-radius: 10px"
            >
              <Toolbar class="w-full py-3">
                <template #start>
                  <div class="flex align-items-center">
                    <Image
                      v-if="checkImg(item.file_path)"
                      :src="basedomainURL + item.file_path"
                      :alt="item.file_name"
                      width="70"
                      height="50"
                      style="
                        object-fit: contain;
                        border: 1px solid #ccc;
                        width: 70px;
                        height: 50px;
                      "
                      preview
                      @error="
                        $event.target.src =
                          basedomainURL + '/Portals/Image/noimg.jpg'
                      "
                      class="pr-2"
                    />
                    <div v-else>
                      <a
                        :href="basedomainURL + item.file_path"
                        download
                        class="w-full no-underline"
                      >
                        <img
                          :src="
                            basedomainURL +
                            '/Portals/Image/file/' +
                            item.file_path.substring(
                              item.file_path.lastIndexOf('.') + 1
                            ) +
                            '.png'
                          "
                          style="width: 70px; height: 50px; object-fit: contain"
                          :alt="item.file_name"
                        />
                      </a>
                    </div>
                    <div class="align-items-center h-full">
                      <div>
                        <a
                          :href="basedomainURL + item.file_path"
                          download
                          class="
                            w-full
                            no-underline
                            text-900
                            align-items-center
                          "
                        >
                          <span class="ml-2 text-900" style="line-height: 50px">
                            {{ item.file_name }}</span
                          >
                        </a>
                      </div>
                    </div>
                  </div>
                </template>
                <template #end> </template>
              </Toolbar>
            </div>
          </div>
        </div>
      </div>
    </div>
  </form>
</template>

<style scoped>
.product-item {
  display: flex;
  align-items: center;
  padding: 0.2rem;
  width: 100%;
}
.product-list-detail {
  flex: 1 1 0;
}

.product-category-icon {
  vertical-align: middle;
  margin-right: 0.5rem;
  font-size: 0.875rem;
}

.product-category {
  vertical-align: middle;
  line-height: 1;
  font-size: 0.875rem;
}

@media screen and (max-width: 576px) {
  .product-item {
    flex-wrap: wrap;
  }
  .image-container {
    width: 100%;
    text-align: center;
  }

  img {
    margin: 0 0 1rem 0;
    width: 100px;
  }
}
</style>
        <style scoped>
.d-lang-header h3,
i {
  font-weight: 600;
}
.d-module-title {
  margin: 0;
}
.d-lang-table {
  margin: 0px 8px 0px 8px;
  height: calc(100vh - 50px);
}

.d-btn-function {
  border-radius: 50%;
  margin-left: 6px;
}
.inputanh {
  border: 1px solid #ccc;
  width: 100%;
  height: 200px;
  cursor: pointer;
  padding: 1px;
  object-fit: contain;
  background-color: #eeeeee;
}
.ipnone {
  display: none;
}

.d-avatar-device_handover {
  position: relative;
  width: 100%;
  height: 350px;
}
.d-avatar-device_handover img {
  position: absolute;
  top: 50%;
  left: 50%;
  transform: translate(-50%, -50%);
  object-fit: contain;
}
.multi-width {
  max-width: 500px !important;
}
.toolbar-filter {
  border: unset;
  outline: unset;
  background-color: #fff;
  padding-bottom: 0px;
}
.sel-placeholder::placeholder {
  text-align: center;
  position: absolute;
  top: 0;
}
</style>
              
    <style lang="scss" scoped>
::v-deep(.p-calendar) {
  .p-button.p-button-icon-only {
    width: 3.5rem !important;
  }
}
</style>
    