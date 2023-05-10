<script>
import { ref, inject, onMounted } from "vue";
import { useToast } from "vue-toastification";
import { FilterMatchMode } from "primevue/api";
import { encr } from "../../../../util/function.js";
import ItemUserComponent from "./ItemUserComp.vue";
import { store } from "../../../../store/store";
export default {
  components: {
    ItemUserComponent,
  },
  props: {
    hrm: Boolean,
    one: Boolean,
    full: Boolean,
    basic: Boolean,
    removes: String,
    users: Array,
    keyuser: String,
    callbackFun: Function,
  },
  setup(props, ctx) {
    const filterconfigDatabases = ref({
      global: { value: null, matchMode: FilterMatchMode.CONTAINS },
    });
    const bgColor = [
      "#F8E69A",
      "#AFDFCF",
      "#F4B2A3",
      "#9A97EC",
      "#CAE2B0",
      "#8BCFFB",
      "#CCADD7",
    ];
    const axios = inject("axios");
    const cryoptojs = inject("cryptojs");
    const toast = useToast();
    const swal = inject("$swal");
    const showLoadding = ref(false);
    const dtUsers = ref([]);
    const selectedUser = ref();
    const listUsers = async () => {
      showLoadding.value = true;
      let strSQL = {
        query: false,
        proc: "dict_list_users",
        par: [
          {
            par: "user_id",
            va: store.getters.user.user_id,
          },
          {
            par: "organization_id",
            va: store.getters.user.organization_id,
          },
          {
            par: "hrm",
            va: props.hrm,
          },
        ],
      };
      try {
        const axResponse = await axios.post(
          baseURL + "/api/HRM_SQL/getData",
          {
            str: encr(JSON.stringify(strSQL), SecretKey, cryoptojs).toString(),
          },
          {
            headers: { Authorization: `Bearer ${store.getters.token}` },
          }
        );

        if (axResponse.status == 200) {
          if (axResponse.data.error) {
            toast.error("Không tải được dữ liệu");
          } else {
            let dts = JSON.parse(axResponse.data.data);
            dtUsers.value = dts[0];
            initSelectU(props.removes);
          }
        }
        showLoadding.value = false;
      } catch (e) {
        console.log(e);
        showLoadding.value = false;
        toast.error("Có lỗi xảy ra, vui lòng thử lại!");
      }
    };
    const choiceUser = () => {
      if (props.full) {
        props.callbackFun(props.keyuser, selectedUser.value);
        return false;
      }
      if (props.hrml) {
        props.callbackFun(
          props.keyuser,
          !selectedUser.value
            ? ""
            : props.one
            ? selectedUser.value["profile_id"]
            : selectedUser.value.map((x) => x.profile_id).join(",")
        );
      } else {
        props.callbackFun(
          props.keyuser,
          !selectedUser.value
            ? ""
            : props.one
            ? selectedUser.value["user_id"]
            : selectedUser.value.map((x) => x.user_id).join(",")
        );
      }
    };
    const baseUrl = fileURL;
    const initSelectU = (users) => {
      if (props.one) {
        if (props.hrm)
          selectedUser.value = dtUsers.value.find((x) =>
            (users + ",").includes(x.profile_id + ",")
          );
        else
          selectedUser.value = dtUsers.value.find((x) =>
            (users + ",").includes(x.user_id + ",")
          );
      } else {
        if (props.hrm)
          selectedUser.value = dtUsers.value.filter((x) =>
            (users + ",").includes(x.profile_id + ",")
          );
        else
          selectedUser.value = dtUsers.value.filter((x) =>
            (users + ",").includes(x.user_id + ",")
          );
      }
    };
    onMounted(() => {
      if (dtUsers.value.length == 0) {
        listUsers();
      }
    });
    ctx.expose({ dtUsers, initSelectU });
    return {
      props,
      baseUrl,
      dtUsers,
      selectedUser,
      initSelectU,
      bgColor,
      choiceUser,
    };
  },
};
</script>
<template>
  <Dropdown
    @change="choiceUser()"
    v-if="props.basic && props.one"
    :virtualScrollerOptions="{ itemSize: 80 }"
    showClear
    v-model="selectedUser"
    :options="dtUsers"
    filter
    optionLabel="full_name"
    placeholder="Chọn người dùng"
    class="w-full"
  >
    <template #value="slotProps">
      <ItemUserComponent
        v-if="slotProps.value"
        :user="slotProps.value"
        :basic="true"
      >
      </ItemUserComponent>
      <div v-else>Chọn nhân sự</div>
    </template>
    <template #option="slotProps">
      <ItemUserComponent
        :user="slotProps.option"
        :size="'large'"
      ></ItemUserComponent>
    </template>
  </Dropdown>
  <MultiSelect
    @change="choiceUser()"
    display="chip"
    v-if="props.basic && !props.one"
    :virtualScrollerOptions="{ itemSize: 80 }"
    showClear
    v-model="selectedUser"
    :options="dtUsers"
    filter
    optionLabel="full_name"
    :maxSelectedLabels="props.full ? 3 : 100"
    placeholder="Chọn người dùng"
    class="w-full align-items-center ov-multiselect"
  >
    <template #chip="slotProps" v-if="!props.full">
      <ItemUserComponent
        :user="slotProps.value"
        :basic="true"
      ></ItemUserComponent>
    </template>
    <template #value="slotProps" v-if="props.full">
      <Tag
        v-if="selectedUser && selectedUser.length > 0"
        severity="success"
        :value="'Đã chọn (' + selectedUser.length + ')'"
      ></Tag>
    </template>
    <template #option="slotProps">
      <ItemUserComponent
        :user="slotProps.option"
        :size="'large'"
      ></ItemUserComponent>
    </template>
  </MultiSelect>
</template>
<style lang="scss" scoped></style>
<style>
.ov-multiselect .p-multiselect-label {
  white-space: break-spaces !important;
}

.ov-multiselect .p-multiselect-token {
  vertical-align: top;
  margin: 5px !important;
}
</style>