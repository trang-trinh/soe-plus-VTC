<script setup>
import { ref, inject, onMounted } from "vue";
import { encr } from "../../../util/function.js";
const cryoptojs = inject("cryptojs");
const axios = inject("axios"); // inject axios
const store = inject("store");
const swal = inject("$swal");
const config = {
  headers: { Authorization: `Bearer ${store.getters.token}` },
};
const basedomainURL = fileURL;
const bgColor = ref([
  "#F8E69A",
  "#AFDFCF",
  "#F4B2A3",
  "#9A97EC",
  "#CAE2B0",
  "#8BCFFB",
  "#CCADD7",
  "#FF88D3",
]);
const height1 = ref(window.screen.availHeight);
const TatCa = ref(false);
const NguoiXuLy = ref(false);
const NguoiDongXuLy = ref(false);
const NguoiTheoDoi = ref(false);
const NguoiQuanLy = ref(false);
const Switch = (e) => {
  switch (e) {
    default:
      TatCa.value = true;
      NguoiXuLy.value = false;
      NguoiDongXuLy.value = false;
      NguoiTheoDoi.value = false;
      NguoiQuanLy.value = false;
      LoadMember(null);
      LoadUser();
      break;
    case "1":
      TatCa.value = true;
      NguoiXuLy.value = false;
      NguoiDongXuLy.value = false;
      NguoiTheoDoi.value = false;
      NguoiQuanLy.value = false;
      LoadMember(null);
      LoadUser();
      break;
    case "2":
      TatCa.value = false;
      NguoiXuLy.value = true;
      NguoiDongXuLy.value = false;
      NguoiQuanLy.value = false;
      NguoiTheoDoi.value = false;
      LoadMember(1);
      LoadUser();
      break;
    case "3":
      TatCa.value = false;
      NguoiXuLy.value = false;
      NguoiDongXuLy.value = true;
      NguoiQuanLy.value = false;
      NguoiTheoDoi.value = false;
      LoadUser();
      LoadMember(2);
      break;
    case "4":
      TatCa.value = false;
      NguoiXuLy.value = false;
      NguoiDongXuLy.value = false;
      NguoiQuanLy.value = false;
      NguoiTheoDoi.value = true;
      LoadUser();
      LoadMember(3);
      break;
    case "5":
      TatCa.value = false;
      NguoiXuLy.value = false;
      NguoiDongXuLy.value = false;
      NguoiQuanLy.value = true;
      NguoiTheoDoi.value = false;
      LoadMember(0);
      LoadUser();
      break;
  }
};
const allMembers = ref();
const ngv = ref();
const nth = ref();
const ndth = ref();
const ntd = ref();
const memberType = ref();
const memberType1 = ref();
const memberType2 = ref();
const memberType3 = ref();
const members = ref();
const props = defineProps({
  id: Intl,
  isType: Intl,
  isClose: Boolean,
});
const isHaveData = ref(false);
const LoadMember = (type) => {
  axios
    .post(
      baseURL + "/api/TaskProc/getTaskData",
      {
        str: encr(
          JSON.stringify({
            proc: "task_member_get",
            par: [
              { par: "id", va: props.id },
              { par: "type", va: null },
            ],
          }),
          SecretKey,
          cryoptojs,
        ).toString(),
      },
      config,
    )
    .then((response) => {
      let mem = JSON.parse(response.data.data)[0];
      let countMem = JSON.parse(response.data.data)[1];
      let countMemType0 = JSON.parse(response.data.data)[3];
      let countMemType1 = JSON.parse(response.data.data)[4];
      let countMemType2 = JSON.parse(response.data.data)[5];
      let countMemType3 = JSON.parse(response.data.data)[6];
      members.value = mem;
      // member;
      ngv.value = countMemType0[0].countType0;
      allMembers.value = countMem[0].count;
      nth.value = countMemType1[0].countType1;
      ndth.value = countMemType2[0].countType2;
      ntd.value = countMemType3[0].countType3;
      tabs.value = [];
      tabs.value.push({ header: "Tất cả (" + allMembers.value + ")", va: "1" });
      tabs.value.push({ header: "Người quản lý (" + ngv.value + ")", va: "5" });
      tabs.value.push({
        header: "Người xử lý chính (" + nth.value + ")",
        va: "2",
      });
      tabs.value.push({
        header: "Người đồng xử lý (" + ndth.value + ")",
        va: "3",
      });
      tabs.value.push({
        header: "Người theo dõi (" + ntd.value + ")",
        va: "4",
      });
      let sttgv = 0;
      let sttth = 0;
      let sttdth = 0;
      let stttd = 0;
      members.value.forEach((element) => {
        element.STTGV = null;
        element.STTTH = null;
        element.STTDTH = null;
        element.STTTD = null;
        element.icon =
          element.is_type == 0
            ? "pi pi-user"
            : element.is_type == 1
            ? "pi pi-user-edit"
            : element.is_type == 2
            ? "pi pi-user-edit"
            : element.is_type == 3
            ? "pi pi-users"
            : "";
        element.tag =
          element.is_type == 0
            ? "Người giao việc"
            : element.is_type == 1
            ? "Người xử lý chính"
            : element.is_type == 2
            ? "Người đồng xử lý"
            : element.is_type == 3
            ? "Người theo dõi"
            : "";
        element.tag_bg =
          element.is_type == 0
            ? "#337ab7"
            : element.is_type == 1 || element.is_type == 2
            ? "#5cb85c"
            : element.is_type == 3
            ? "#5bc0de"
            : "";
        element.tooltip =
          element.full_name +
          ("<br/>" + (element.positions ?? "") != "<br/>"
            ? "<br/>" + (element.positions ?? "")
            : "") +
          "<br/>" +
          (element.department_name != null
            ? element.department_name
            : element.organiztion_name);

        if (element.is_type == 0) {
          element.STTGV = sttgv;
          sttgv++;
        }
        if (element.is_type == 1) {
          element.STTTH = sttth;
          sttth++;
        }
        if (element.is_type == 2) {
          element.STTDTH = sttdth;
          sttdth++;
        }
        if (element.is_type == 3) {
          element.STTTD = stttd;
          stttd++;
        }

        if (store.state.user.user_id == element.user_id) {
          if (memberType.value == null) {
            memberType.value = element.is_type;
            return;
          }
          if (memberType1.value == null) {
            memberType1.value = element.is_type;
            return;
          }
          if (memberType2.value == null) {
            memberType2.value = element.is_type;
            return;
          }
          if (memberType3.value == null) {
            memberType3.value = element.is_type;
            return;
          }
        }
      });
      var byDate = members.value.slice(0);

      byDate.sort(function (a, b) {
        return a.is_type - b.is_type;
      });
      let listUser = [];
      byDate.forEach((x) => {
        let userss = {
          avt: x.avt,
          department_name: x.department_name,
          full_name: x.full_name,
          organiztion_name: x.organiztion_name,
          positions: x.positions,
          tooltip: x.tooltip,
          user_id: x.user_id,
        };
        if (listUser.filter((y) => y.user_id === userss.user_id).length < 1) {
          listUser.push(userss);
        }
      });
      listUser.forEach((x) => {
        x.listType = [];
        let filter = byDate.filter((a) => a.user_id === x.user_id);
        x.data = JSON.parse(JSON.stringify(filter));
   
        if (filter.length > 0) {
          filter.forEach((k) => {
            x.listType.push(k.is_type);
          });
        }
      });

      if (type == null) {
        members.value = JSON.parse(JSON.stringify(listUser));
      } else {
        members.value = JSON.parse(
          JSON.stringify(
            listUser.filter((x) => x.listType.includes(type) == true),
          ),
        );
      }

      isHaveData.value = members.value.length > 0 ? true : false;
    })
    .catch((error) => {
      // toast.error("Tải dữ liệu không thành công!");
      if (error && error.status === 401) {
        swal.fire({
          text: "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
      }
    });
};
const ListUser = ref([]);
const LoadUser = () => {
  //   if (event == null) {
  //     event.query = null;
  //   }
  ListUser.value = [];
  let type =
    TatCa.value == true
      ? null
      : NguoiQuanLy.value == true
      ? 0
      : NguoiXuLy.value == true
      ? 1
      : NguoiDongXuLy.value == true
      ? 2
      : NguoiTheoDoi.value == true
      ? 3
      : null;
  axios
    .post(
      baseURL + "/api/TaskProc/getTaskData",
      {
        str: encr(
          JSON.stringify({
            proc: "task_member_Select",
            par: [
              { par: "user_id", va: store.state.user.user_id },
              { par: "search", va: null },
              { par: "task_id", va: props.id },
              { par: "type", va: type },
            ],
          }),
          SecretKey,
          cryoptojs,
        ).toString(),
      },
      config,
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];
      data.forEach((x) => {
        x.fname = x.full_name;
      });
      ListUser.value = data;
    })
    .catch((error) => {
      // toast.error("Tải dữ liệu không thành công!");
      if (error && error.status === 401) {
        swal.fire({
          text: "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
      }
    });
};
const tabs = ref([]);
const indexTab = ref();
const changeTab = (e) => {
  Switch(tabs.value[e].va);
  indexTab.value = tabs.value[e].va;
};
onMounted(() => {
  Switch(props.isType ? props.isType : 1);
});
</script>
<template>
  <div>
    <div class="grid relative">
      <TabView
        @tab-change="changeTab($event.index)"
        class="w-full px-3"
      >
        <TabPanel
          v-for="(item, index) in tabs"
          :key="index"
          :header="item.header"
        >
          <div class="row col-12">
            <ScrollPanel
              :style="
                height1 < 1000
                  ? 'height: calc(78vh) !important'
                  : 'height: calc(80vh) !important'
              "
              v-if="isHaveData == true"
            >
              <div
                class="row col-12 flex p-0 m-0 my-div"
                v-for="(m, index) in members"
                :key="m"
                style="border-bottom: 1px solid #ccc"
              >
                <div class="col-1 format-center p-0 m-0">
                  <Avatar
                    @error="
                      $event.target.src =
                        basedomainURL + '/Portals/Image/nouser1.png'
                    "
                    v-tooltip.right="{
                      value: m.tooltip,
                      escape: true,
                      fitContent: true,
                    }"
                    v-bind:label="
                      m.avt ? '' : m.full_name.split(' ').at(-1).substring(0, 1)
                    "
                    v-bind:image="basedomainURL + m.avt"
                    style="color: #ffffff; cursor: pointer"
                    :style="{
                      background: bgColor[index % 7],
                      border: '2px solid' + bgColor[index % 7],
                    }"
                    class="col-2 p-0 m-0"
                    size="large"
                    shape="circle"
                  />
                </div>
                <div class="col-9">
                  <div class="col-12">
                    <span class="font-bold text-xl text-indigo-700">{{
                      m.full_name
                    }}</span>
                  </div>
                  <div class="col-12 py-1">{{ m.positions }}</div>
                  <div class="col-12 py-1">
                    {{
                      m.department_name ? m.department_name : m.organiztion_name
                    }}
                  </div>
                  <div class="col-12 py-1">
                    <span
                      class="tag-custom"
                      v-for="(item, indexItem) in m.data"
                      :key="indexItem"
                    >
                      <Tag
                        :style="{ background: item.tag_bg }"
                        :icon="item.icon"
                        :value="item.tag"
                      ></Tag>
                    </span>
                  </div>
                </div>
              </div>
            </ScrollPanel>
            <div
              class="row col-12 align-items-center justify-content-center p-4 text-center m-auto"
              v-else
            >
              <img
                src="../../../assets/background/nodata.png"
                height="144"
              />
              <h3 class="m-1">Không có dữ liệu</h3>
            </div>
          </div>
        </TabPanel>
      </TabView>
    </div>
  </div>
</template>
<style scoped>
.format-center {
  display: flex;
  justify-content: center;
  align-items: center;
  vertical-align: middle;
  text-align: center;
}

.format-right {
  display: flex;
  justify-content: flex-end;
  align-items: center;
  vertical-align: middle;
  text-align: center;
}

.format-left {
  display: flex;
  justify-content: flex-start;
  align-items: center;
  vertical-align: middle;
  text-align: center;
}
.activated {
  background: #f5f5f5 !important;
  color: #2196f3 !important;
  border-color: transparent;
  font-weight: 600;
}
.my-div:hover {
  background-color: #e5f3ff;
}
.p-custom {
  font-size: 125%;
  background-color: #ffffff !important;
  color: black;
}
.p-button-text-custom {
  color: black;
  font-weight: 500;
}
</style>
<style lang="scss" scoped>
::v-deep(.p-autocomplete-panel) {
  &.my-panel {
    max-height: 30vh !important;
  }
}
.tag-custom {
  margin: 0rem 1rem;
}
.tag-custom:first-child {
  margin-left: 0;
}
</style>
