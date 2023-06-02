<script setup>
import { ref, inject, onMounted } from "vue";
import { useToast } from "vue-toastification";
import { encr } from "../../../util/function.js";
import moment from "moment";
const cryoptojs = inject("cryptojs");
const axios = inject("axios"); // inject axios
const store = inject("store");
const swal = inject("$swal");
const toast = useToast();
const emitter = inject("emitter");
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
const model = ref([]);
const configMember = ref();
const user = store.getters.user;
const loadConfig = () => {
  axios
    .post(
      baseURL + "/api/TaskProc/getTaskData",
      {
        str: encr(
          JSON.stringify({
            proc: "task_review_member_config_get",
            par: [{ par: "user_id", va: user.user_id }],
          }),
          SecretKey,
          cryoptojs
        ).toString(),
      },
      config
    )
    .then((response) => {
      let data = JSON.parse(response.data.data)[0];
      configMember.value =
        data.length > 0
          ? data[0]
          : { manager: 0, executors: 0, co_executors: 0, supervisor: 0 };
    })
    .catch((error) => {
      toast.error("Tải dữ liệu không thành công!");
      if (error && error.status === 401) {
        swal.fire({
          text: "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại!",
          confirmButtonText: "OK",
        });
        store.commit("gologout");
      }
    });
};
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
const avgg = function (items, prop) {
  return items.reduce(function (a, b) {
    return a + b[prop];
  }, 0);
};
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
          cryoptojs
        ).toString(),
      },
      config
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
          x.sumAvg = avgg(filter, "point") / filter.length;
        } else x.sumAvg = null;
      });

      if (type == null) {
        members.value = JSON.parse(JSON.stringify(listUser));
      } else {
        members.value = JSON.parse(
          JSON.stringify(
            listUser.filter((x) => x.listType.includes(type) == true)
          )
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
const selectedUser = ref();
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
          cryoptojs
        ).toString(),
      },
      config
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
const filteredItems = ref([]);
const searchItems = (event) => {
  //in a real application, make a request to a remote url with the query and return filtered results, for demo we filter at client side
  let query =
    event.query.includes("@") == true
      ? event.query.replace("@", "")
      : event.query;
  let filteItems = [];
  for (let i = 0; i < ListUser.value.length; i++) {
    let item = ListUser.value[i];

    if (
      item.user_id.toLowerCase().indexOf(query.toLowerCase()) === 0 ||
      item.full_name.includes(query) == true
    ) {
      filteItems.push(item);
    }
  }
  filteredItems.value = filteItems;
};
const TaskMember = ref({
  task_id: props.id,
  user_id: null,
  is_type: null,
  status: 1,
});
const addMember = (event) => {
  let type =
    NguoiXuLy.value == true
      ? 1
      : NguoiDongXuLy.value == true
      ? 2
      : NguoiTheoDoi.value == true
      ? 3
      : null;
  TaskMember.value = {
    task_id: props.id,
    user_id: event.value.user_id,
    is_type: type,
    status: 1,
  };
  selectedUser.value = "";
  axios
    .post(baseURL + "/api/task_Member/Add_task_Member", TaskMember.value, {
      headers: { Authorization: `Bearer ${store.getters.token}` },
    })
    .then((response) => {
      if (response.data.err != "1") {
        swal.close();
        toast.success("Thêm thành viên thành công!");
        LoadMember(type);
        LoadUser(type);
        emitter.emit("addMember", true);
      } else {
        let ms = response.data.ms;
        swal.fire({
          title: "Thông báo!",
          html: ms,
          icon: "error",
          confirmButtonText: "OK",
        });
      }
    })
    .catch((error) => {
      swal.close();
      swal.fire({
        title: "Thông báo",
        text: "Có lỗi xảy ra, vui lòng kiểm tra lại!" + error,
        icon: "error",
        confirmButtonText: "OK",
      });
    });
};
const Update_Point = () => {
  let formData = new FormData();
  let list = [];
  model.value.forEach((m) => {
    if (m.data.length > 0)
      m.data.forEach((d) => {
        d.point = m.sumAvg;
        d.comments = m.comments;
        list.push(d);
      });
  });

  formData.append("member", JSON.stringify(list));
  axios
    .put(baseURL + "/api/task_Member/Update_Member_Info", formData, {
      headers: { Authorization: `Bearer ${store.getters.token}` },
    })
    .then((response) => {
      if (response.data.err != "1") {
        toast.success("Cập nhật thành viên công việc thành công!");
        visibleDialog.value = false;
        Switch(indexTab.value);
      } else {
        let ms = response.data.ms;
        swal.fire({
          title: "Thông báo!",
          html: ms,
          icon: "error",
          confirmButtonText: "OK",
        });
      }
    })
    .catch((error) => {
      swal.close();
      swal.fire({
        title: "Thông báo",
        text: "Có lỗi xảy ra, vui lòng kiểm tra lại!" + error,
        icon: "error",
        confirmButtonText: "OK",
      });
    });
};
const delMember = (user) => {
  let type =
    NguoiXuLy.value == true
      ? 1
      : NguoiDongXuLy.value == true
      ? 2
      : NguoiTheoDoi.value == true
      ? 3
      : null;
  let id = [];
  if (type != null) {
    id.push(user.data.filter((x) => x.is_type === type)[0].member_id);
  } else {
    user.data
      .filter((x) => x.is_type != type)
      .forEach((u) => {
        id.push(u.member_id);
      });
  }
  swal
    .fire({
      title: "Thông báo",
      text: "Bạn có muốn xoá thành viên này không!",
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
          .delete(baseURL + "/api/task_Member/Delete_task_Member", {
            headers: { Authorization: `Bearer ${store.getters.token}` },
            data: id,
          })
          .then((response) => {
            swal.close();
            if (response.data.err != "1") {
              swal.close();
              toast.success("Xoá thành viên thành công!");
              let type =
                NguoiXuLy.value == true
                  ? 1
                  : NguoiDongXuLy.value == true
                  ? 2
                  : NguoiTheoDoi.value == true
                  ? 3
                  : null;
              LoadMember(type);
              LoadUser(type);
              emitter.emit("delMember", true);
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
            if (error.status === 401) {
              swal.fire({
                text: "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại!",
                confirmButtonText: "OK",
              });
            }
          });
      }
    });
};
const tabs = ref([]);
const indexTab = ref();
const changeTab = (e) => {
  Switch(tabs.value[e].va);
  indexTab.value = tabs.value[e].va;
};
const headerDialog = ref();
const visibleDialog = ref(false);
const Point = (e) => {
  visibleDialog.value = true;
  is_multiple.value = true;
  headerDialog.value = "Đánh giá thành viên";
  model.value = JSON.parse(JSON.stringify(members.value.filter((x) => x == e)));
  let arr = Object.values(configMember.value);
  arr = arr.filter((x) => typeof x === "number");
  let min = Math.min(...arr);
  let max = Math.max(...arr);
  model.value.forEach((z) => {
    z.sumAvg = z.sumAvg > 0 ? z.sumAvg : max;
  });
};
onMounted(() => {
  Switch(props.isType ? props.isType : 1);
  loadConfig();
});
const is_multiple = ref(false);
const OpenMultiple = () => {
  visibleDialog.value = true;
  headerDialog.value = "Đánh giá thành viên";
  is_multiple.value = true;
  model.value = [];
  model.value = JSON.parse(JSON.stringify(members.value));
  let arr = Object.values(configMember.value);
  let min = Math.min(...arr);
  let max = Math.max(...arr);
  model.value.forEach((z) => {
    z.sumAvg = z.sumAvg > 0 ? z.sumAvg : max;
  });
};
</script>
<template>
  <div>
    <div class="grid relative">
      <Button
        class="right-0 absolute z-5 my-2 mx-3"
        label="Đánh giá"
        icon="pi pi-user-edit"
        @click="OpenMultiple()"
        v-if="
          memberType == 0 ||
          memberType1 == 0 ||
          memberType2 == 0 ||
          memberType3 == 0
        "
      >
      </Button>

      <TabView @tab-change="changeTab($event.index)" class="w-full px-3">
        <TabPanel
          v-for="(item, index) in tabs"
          :key="index"
          :header="item.header"
        >
          <div class="row col-12">
            <AutoComplete
              v-model="selectedUser"
              :suggestions="filteredItems"
              @complete="searchItems"
              class="row w-full col-12"
              placeholder="@ để thêm người"
              :panelClass="'my-panel'"
              :dropdown="true"
              scrollHeight="700px"
              @item-select="addMember($event)"
              v-if="
                props.isClose == false &&
                !TatCa &&
                !NguoiQuanLy &&
                (memberType == 0 ||
                  memberType1 == 0 ||
                  memberType2 == 0 ||
                  memberType3 == 0)
              "
            >
              <template #item="slotProps">
                <div
                  class="col-12 flex align-items-center"
                  style="border-bottom: 1px solid #ccc"
                >
                  <div class="col-2 format-center">
                    <Avatar
                      @error="
                        $event.target.src =
                          basedomainURL + '/Portals/Image/nouser1.png'
                      "
                      :label="
                        slotProps.item.avatar
                          ? ''
                          : slotProps.item.fname
                              .split(' ')
                              .at(-1)
                              .substring(0, 1)
                      "
                      v-bind:image="basedomainURL + slotProps.item.avatar"
                      style="color: #ffffff; cursor: pointer"
                      :style="{
                        background: bgColor[Math.floor(Math.random() * 10) % 7],
                        border: '2px solid' + bgColor[(Math.random() * 11) % 7],
                      }"
                      class="col-2 p-0 m-0"
                      size="large"
                      shape="circle"
                    />
                  </div>
                  <div class="col-9">
                    <div class="col-12">
                      <span class="font-bold text-xl text-indigo-700">
                        {{ slotProps.item.fname }}
                      </span>
                    </div>
                    <div class="col-12 py-1">
                      {{ slotProps.item.tenChucVu }}
                    </div>
                    <div class="col-12 py-1">
                      {{
                        slotProps.item.department_name
                          ? slotProps.item.department_name
                          : slotProps.item.organiztion_name
                      }}
                    </div>
                  </div>
                </div>
              </template>
            </AutoComplete>
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
                <div class="col-8">
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
                <div class="col-1 flex align-items-center justify-content-end">
                  <span v-if="m.sumAvg > 0">{{ m.sumAvg + " %" }}</span>
                </div>
                <div
                  class="col-2 flex align-items-center justify-content-end"
                  v-if="
                    props.isClose == false &&
                    (memberType == 0 ||
                      memberType1 == 0 ||
                      memberType2 == 0 ||
                      memberType3 == 0)
                  "
                >
                  <div>
                    <Button
                      icon="p-custom pi pi-user-edit"
                      class="p-button-raised p-button-text p-custom mx-1"
                      v-tooltip="'Đánh giá thành viên'"
                      @click="Point(m)"
                    />
                    <Button
                      v-if="m.listType.includes(0) == false"
                      icon="p-custom pi pi-trash"
                      class="p-button-raised p-button-text p-custom mx-1"
                      @click="delMember(m, indexTab)"
                      v-tooltip="'Xóa thành viên'"
                    />
                    <div
                      v-else
                      class="h-full p-button p-component p-button-text mx-1"
                    ></div>
                  </div>
                </div>
              </div>
            </ScrollPanel>
            <div
              class="row col-12 align-items-center justify-content-center p-4 text-center m-auto"
              v-else
            >
              <img src="../../../assets/background/nodata.png" height="144" />
              <h3 class="m-1">Không có dữ liệu</h3>
            </div>
          </div>
        </TabPanel>
      </TabView>
    </div>
  </div>
  <Dialog
    v-model:visible="visibleDialog"
    :header="headerDialog"
    modal
    :closable="false"
    dismissableMask
    style="width: 55vw"
  >
    <form v-if="is_multiple == false">
      <div class="col-12 flex">
        <div class="col-4 flex align-items-center">Điểm đánh giá</div>
        <InputNumber
          class="col-8"
          suffix=" %"
          mode="decimal"
          :minFractionDigits="2"
          :useGrouping="false"
          v-model="model.point"
          v-tooltip="'Mức hoàn thành công việc'"
        >
        </InputNumber>
      </div>

      <div class="col-12 flex">
        <div class="col-4 flex align-items-center">Nội dung</div>
        <div class="col-8">
          <Textarea
            class="w-full"
            autoResize
            v-model="model.comments"
          ></Textarea>
        </div>
      </div>
    </form>
    <form v-else>
      <div
        class="row col-12 flex p-0 m-0"
        style="border-bottom: 1px solid #ccc"
      >
        <div class="col-1 format-center p-0 m-0"></div>
        <div class="col-6 text-center">Thông tin thành viên</div>
        <div class="col-2 text-center">Điểm đánh giá</div>
        <div class="col-3 text-center">Nội dung đánh giá</div>
      </div>
      <div
        class="row col-12 flex p-0 m-0 my-div"
        v-for="(m, index) in model"
        :key="m"
        style="border-bottom: 1px solid #ccc"
      >
        <div class="col-1 format-center p-0 m-0">
          <Avatar
            @error="
              $event.target.src = basedomainURL + '/Portals/Image/nouser1.png'
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
        <div class="col-6">
          <div class="col-12">
            <span class="font-bold text-xl text-indigo-700">{{
              m.full_name
            }}</span>
          </div>
          <div class="col-12 py-1">{{ m.positions }}</div>
          <div class="col-12 py-1">
            {{ m.department_name ? m.department_name : m.organiztion_name }}
          </div>
          <div class="col-12 py-1">
            <span
              class="mx-1"
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

        <InputNumber
          class="col-2 format-center"
          suffix=" %"
          mode="decimal"
          :minFractionDigits="2"
          :useGrouping="false"
          v-model="m.sumAvg"
          v-tooltip="'Mức hoàn thành công việc'"
        >
        </InputNumber>

        <div class="col-3 format-center">
          <Textarea
            class="w-full h-full"
            autoResize
            v-model="m.comments"
          ></Textarea>
        </div>
      </div>
    </form>
    <template #footer>
      <Button
        class="mx-1 p-button-text"
        icon="pi pi-times"
        label="Hủy"
        @click="visibleDialog = false"
      ></Button>
      <Button
        class="mx-1"
        icon="pi pi-check"
        label="Lưu"
        @click="Update_Point()"
      ></Button>
    </template>
  </Dialog>
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
