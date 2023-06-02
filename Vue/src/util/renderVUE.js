const renderVUE = (table, columns) => {
  let primarykey = columns[0];
  let fileColumns = columns.filter(
    (x) => x.Input == "Avarta" || x.Input == "Image" || x.Input == "File"
  );
  let requiredColumns = columns.filter((x) => x.Required);
  let formColumns = columns.filter((x) => x.ShowForm);
  let columnKey = columns.find((x) => x.IsKey == true);
  let columnValue = columns.find((x) => x.IsValue == true);
  let columnTrangthai = columns.find((x) => x.Name == "Trangthai");
  let columnTrangthaiType =
    columnTrangthai.CType == "int"
      ? "int"
      : columnTrangthai.CType == "bit"
      ? "bool"
      : "string";
  let dmodel = primarykey.Name.split("_")[0];
  let model = dmodel.toLowerCase();
  let tableTitle = table.Title;
  let tableTitleLower = table.Title.toLowerCase();
  let tableTitleUpper = table.Title.toUpperCase();
  function InputForm(cl) {
    let pkey = primarykey.Name == cl.Name;
    let rq = cl.Required;
    let field = `
      <InputText
      ${pkey ? ':disabled="!isAdd"' : ""}
      spellcheck="false"
      class="col-9 ip36"
      v-model="${model}.${cl.Name}"
      ${
        rq ? `:class="{'p-invalid': v$.${cl.Name}.$invalid && submitted }"` : ""
      }
      />
      `;
    switch (cl.Input) {
        case "Text":
            break;
        case "Number":
            field = `<InputNumber class="col-9 ip36 p-0" v-model="${model}.${cl.Name}" />`;
            break;
        case "Switch":
            field = `<InputSwitch v-model="${model}.${cl.Name}" style="vertical-align: bottom;" />`;
            break;
        default:
            break;
    }
    return field;
  }
  function columnRequired(cl) {
    let field = `
<div class="field ${cl.Css} md:${cl.Css}">
    <label class="col-3 text-left">${
      cl.Title
    } <span class="redsao">(*)</span></label>
    ${InputForm(cl)}
</div>

<small
    v-if="
    (v$.${cl.Name}.$invalid && submitted) ||
    v$.${cl.Name}.$pending.$response
    "
    class="col-9 p-error"
>
    <div class="field ${cl.Css} md:${cl.Css}">
    <label class="col-3 text-left"></label>
    <span class="col-9 pl-3">{{
        v$.${cl.Name}.required.$message
        .replace("Value", "${cl.Title}")
        .replace("is required", "không được để trống")
    }}</span>
    </div></small
>
    `;
    return field;
  }
  function columnField(cl) {
    let field = `
<div class="field ${cl.Css} md:${cl.Css}">
    <label class="col-3 text-left">${cl.Title}</label>
    ${InputForm(cl)}
</div>
    `;
    return field;
  }
  function formColumn() {
    let fields = [];
    formColumns.forEach((cl) => {
      if (cl.Required) {
        fields.push(columnRequired(cl));
      } else fields.push(columnField(cl));
    });
    return fields.join("\n");
  }
  let style = `
<style scoped>
${
  fileColumns.length > 0
    ? `.ipnone {
    display: none;
}
.inputanh {
    border: 1px solid #ccc;
    width: 96px;
    height: 96px;
    cursor: pointer;
    padding: 1px;
}
.inputanh img {
    object-fit: cover;
    width: 100%;
    height: 100%;
}`
    : ""
}
</style>
    `;
  let strVUE = `
<script setup>
import { ref, inject, onMounted } from "vue";
import { required } from "@vuelidate/validators";
import { useToast } from "vue-toastification";
import { useVuelidate } from "@vuelidate/core";
//init Model
const ${model} = ref({
    STT: 1,
    Trangthai: ${columnTrangthaiType == "int" ? "1" : "true"},
});
//Valid Form
const submitted = ref(false);
const rules = {
${requiredColumns.map((cl) => `${cl.Name}: {required}`).join(",\n")}
};
const v$ = useVuelidate(rules, ${model});
//Khai báo biến
const isAdd = ref(true);
const selectedNodes = ref([]);
const filters = ref({});
const opition = ref({ search: "" });
const ${model}s = ref();
const displayAdd${dmodel} = ref(false);
const isFirst = ref(true);
const toast = useToast();
const store = inject("store");
const swal = inject("$swal");
const axios = inject("axios"); // inject axios
const basedomainURL = fileURL;
const BearerToken = { Authorization: "Bearer " + store.getters.token };
const config = {
    headers: BearerToken,
};
const swalLoadding = () => {
    swal.fire({
      width: 110,
      didOpen: () => {
        swal.showLoading();
      },
    });
  };
const errorMessage = () => {
    swal.fire({
        title: "Error!",
        text: "Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại!",
        icon: "error",
        confirmButtonText: "OK",
    });
};
const swalMessage = (title, icon, ms) => {
    swal.fire({
        title: title,
        text: ms,
        icon: icon,
        confirmButtonText: "OK",
    });
};
const menuButs = ref();
const itemButs = ref([
    {
        label: "Xuất Excel",
        icon: "pi pi-file-excel",
        command: () => {
            export${dmodel}("ExportExcel");
        },
    },
    {
        label: "Xuất Mẫu",
        icon: "pi pi-file-excel",
        command: () => {
            export${dmodel}("ExportExcelMau");
        },
    },
]);

//Khai báo function
const toggleExport = (event) => {
    menuButs.value.toggle(event);
};
//Show Modal
const closedisplayAdd${dmodel} = () => {
    displayAdd${dmodel}.value = false;
};
const showModalAdd${dmodel} = () => {
    isAdd.value = true;
    submitted.value = false;
    ${model}.value = {
        STT: ${model}s.value.length + 1,
        Trangthai: true,
    };
    displayAdd${dmodel}.value = true;
};
${
  fileColumns.length > 0
    ? `
let files = [];
const chonanh = (id) => {
    document.getElementById(id).click();
};
const handleFileUpload = (event) => {
    files = event.target.files;
};
`
    : ""
}

//Thêm sửa xoá
const onRefersh = () => {
    opition.value.search = "";
    load${dmodel}(true);
};
const onSearch = () => {
    load${dmodel}(true);
};
const load${dmodel} = (rf) => {
    if (rf) {
        opition.value.loading = true;
    }
    axios
    .post(
        baseURL + "/api/Proc/CallProc",
        { proc: "${
          table.TABLE_NAME
        }_List", par: [{ par: "s", va: opition.value.search }] },
        config
    )
    .then((response) => {
        let data = JSON.parse(response.data.data)[0];
        if (data.length > 0) {
            ${model}s.value = data;
        } else {
            ${model}s.value = [];
        }
        if (isFirst.value) isFirst.value = false;
        if (rf) {
            opition.value.loading = false;
        }
    })
    .catch((error) => {
        if (error && error.status === 401) {
            errorMessage();
        }
    });
};
const edit${dmodel} = (md) => {
    isAdd.value = false;
    submitted.value = false;
    swalLoadding();
    displayAdd${dmodel}.value = true;
    axios
    .post(
        baseURL + "/api/Proc/CallProc",
        {
        proc: "${table.TABLE_NAME}_Get",
        par: [{ par: "${primarykey.Name}", va: md.${primarykey.Name} }],
        },
        config
    )
    .then((response) => {
        swal.close();
        let data = JSON.parse(response.data.data);
        if (data.length > 0) {
            ${model}.value = data[0][0];
        }
    })
    .catch((error) => {
        if (error.status === 401) {
            errorMessage();
        }
    });
};
const handleSubmit = (isFormValid) => {
    submitted.value = true;
    if (!isFormValid) {
        return;
    }
    add${dmodel}();
};
const add${dmodel} = () => {
    swalLoadding();
    axios({
    method: isAdd.value == false ? "put" : "post",
    url:
        baseURL +
        "/api/${dmodel}/" +
        (isAdd.value == false ? "Update_${dmodel}" : "Add_${dmodel}"),
    data: ${model}.value,
    headers: BearerToken,
    })
    .then((response) => {
        if (response.data.err != "1") {
            swal.close();
            toast.success("Cập nhật ${tableTitleLower} thành công!");
            load${dmodel}();
            closedisplayAdd${dmodel}();
        } else {
        swal.fire({
            title: "Error!",
            text: response.data.ms,
            icon: "error",
            confirmButtonText: "OK",
        });
        }
    })
    .catch(() => {
        swal.close();
        swalMessage("Error!","error","Có lỗi xảy ra, vui lòng kiểm tra lại!");
    });
};

const del${dmodel} = (md) => {
    swal
    .fire({
        title: "Thông báo",
        text: "Bạn có muốn xoá ${tableTitleLower} này không!",
        icon: "warning",
        showCancelButton: true,
        confirmButtonColor: "#3085d6",
        cancelButtonColor: "#d33",
        confirmButtonText: "Có",
        cancelButtonText: "Không",
    })
    .then((result) => {
        if (result.isConfirmed) {
            swalLoadding();
            axios
                .delete(baseURL + "/api/${dmodel}/Del_${dmodel}", {
                headers: BearerToken,
                data:
                    md != null
                    ? [md.${primarykey.Name}]
                    : selectedNodes.value.map((x) => x.${primarykey.Name}),
                })
                .then((response) => {
                swal.close();
                if (response.data.err != "1") {
                    swal.close();
                    toast.success("Xoá ${tableTitleLower} thành công!");
                    load${dmodel}();
                    if (!md) selectedNodes.value = [];
                } else {
                    swalMessage("Error!","error",response.data.ms);
                }
                })
                .catch((error) => {
                    swal.close();
                    if (error.status === 401) {
                        errorMessage();
                    }
                });
        }
    });
};

const upTrangthai${dmodel} = (md) => {
    let ids = [md.${primarykey.Name}];
    let tts = [md.Trangthai];
    swalLoadding();
    axios({
    method: "put",
    url: baseURL + "/api/${dmodel}/Update_Trangthai${dmodel}",
    data: { ids: ids, tts: tts },
    headers: BearerToken,
    })
    .then((response) => {
        swal.close();
        if (response.data.err != "1") {
        swal.close();
        toast.success("Cập nhật trạng thái ${tableTitleLower} thành công!");
        load${dmodel}();
        if (!md) selectedNodes.value = [];
        } else {
            swalMessage("Error!","error",response.data.ms);
        }
    })
    .catch((error) => {
        swal.close();
        if (error.status === 401) {
            errorMessage();
        }
    });
};

const export${dmodel} = (method) => {
    swalLoadding();
    let par = [{ par: "name", va: "DM_${dmodel}" }];
    if (method != "ExportExcelMau") {
    par = [{ par: "user_id", va: opition.value.user_id }];
    }
    axios
    .post(
        baseURL + "/api/Excel/" + method,
        {
            excelname: "DANH SÁCH ${tableTitleUpper}",
            proc: "DM_${dmodel}_ListExport",
            par: par,
        },
        config
    )
    .then((response) => {
        swal.close();
        if (response.data.err != "1") {
        swal.close();
        toast.success("Kết xuất ${tableTitle} thành công!");
        window.open(baseURL + response.data.path);
        } else {
            swalMessage("Error!","error",response.data.ms);
        }
    })
    .catch((error) => {
        if (error.status === 401) {
            errorMessage();
        }
    });
};
const filteredItems = ref([]);

onMounted(() => {
    //init
    load${dmodel}(true);
    return {};
});
</script>
<template>
    <div class="main-layout flex-grow-1 p-2" v-if="store.getters.islogin">
    <DataTable
        class="w-full p-datatable-sm e-sm"
        :value="${model}s"
        :paginator="${model}s && ${model}s.length > 20"
        :loading="opition.loading"
        :rows="20"
        dataKey="${primarykey.Name}"
        :showGridlines="true"
        :rowHover="true"
        v-model:selection="selectedNodes"
        :filters="filters"
        filterMode="lenient"
        paginatorTemplate="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown"
        :rowsPerPageOptions="[10, 25, 50]"
        currentPageReportTemplate="Showing {first} to {last} of {totalRecords} entries"
        responsiveLayout="scroll"
        :scrollable="true"
        scrollHeight="flex"
    >
        <template #header>
        <h3 class="module-title mt-0 ml-1 mb-2">
            <i class="pi pi-list"></i> ${tableTitle}
            <span v-if="${model}s">({{ ${model}s.length }})</span>
        </h3>
        <Toolbar class="w-full custoolbar">
            <template #start>
            <span class="p-input-icon-left">
                <i class="pi pi-search" />
                <InputText
                type="text"
                spellcheck="false"
                v-model="opition.search"
                placeholder="Tìm kiếm"
                v-on:keyup.enter="onSearch"
                />
            </span>
            </template>

            <template #end>
            <Button
                class="mr-2 p-button-outlined p-button-secondary"
                icon="pi pi-refresh"
                @click="onRefersh"
            />
            <Button
                label="Xoá"
                icon="pi pi-trash"
                class="mr-2 p-button-danger"
                v-if="selectedNodes.length > 0"
                @click="del${dmodel}()"
            />
            <Button
                label="Export"
                icon="pi pi-file-excel"
                class="mr-2 p-button-outlined p-button-secondary"
                @click="toggleExport"
                aria-haspopup="true"
                aria-controls="overlay_Export"
            />
            <Menu id="overlay_Export" ref="menuButs" :model="itemButs" :popup="true" />
            <Button
                label="Thêm ${tableTitleLower}"
                icon="pi pi-plus"
                class="mr-2"
                @click="showModalAdd${dmodel}"
            />
            </template>
        </Toolbar>
        </template>
        <Column
        selectionMode="multiple"
        headerStyle="text-align:center;max-width:50px"
        bodyStyle="text-align:center;max-width:50px"
        class="align-items-center justify-content-center text-center"
        ></Column>
        <Column
        :sortable="true"
        field="STT"
        header="STT"
        class="align-items-center justify-content-center text-center"
        headerStyle="text-align:center;max-width:100px"
        bodyStyle="text-align:center;max-width:100px"
        ></Column>
        <Column
        field="${primarykey.Name}"
        :sortable="true"
        header="Mã loại"
        class="align-items-center justify-content-center text-center"
        headerStyle="text-align:center;max-width:150px"
        bodyStyle="text-align:center;max-width:150px"
        ></Column>
        <Column field="${
          columnValue.Name
        }" header="Tên ${tableTitleLower}" :sortable="true"></Column>
        <Column
        field="Trangthai"
        header="Trạng thái"
        class="align-items-center justify-content-center text-center"
        headerStyle="text-align:center;max-width:120px"
        bodyStyle="text-align:center;max-width:120px"
        >
        <template #body="md">
            <Checkbox
            v-model="md.data.Trangthai"
            :binary="true"
            @change="upTrangthai${dmodel}(md.data)"
            />
        </template>
        </Column>
        <Column
        headerClass="text-center"
        bodyClass="text-center"
        headerStyle="text-align:center;max-width:120px"
        bodyStyle="text-align:center;max-width:120px"
        >
        <template #header> </template>
        <template #body="md">
            <Button
            type="button"
            icon="pi pi-pencil"
            class="p-button-rounded p-button-sm p-button-info"
            style="margin-right: 0.5rem"
            @click="edit${dmodel}(md.data)"
            ></Button>
            <Button
            type="button"
            icon="pi pi-trash"
            class="p-button-rounded p-button-sm p-button-danger"
            @click="del${dmodel}(md.data)"
            ></Button>
        </template>
        </Column>
        <template #empty>
        <div
            class="m-auto align-items-center justify-content-center p-4 text-center"
            v-if="!isFirst"
        >
            <img src="../../assets/background/nodata.png" height="144" />
            <h3 class="m-1">Không có dữ liệu</h3>
        </div>
        </template>
    </DataTable>
    </div>
    <Dialog
    header="Cập nhật ${tableTitleLower}"
    v-model:visible="displayAdd${dmodel}"
    :style="{ width: '540px', zIndex: 1000 }"
    :maximizable="true"
    :autoZIndex="false"
    :modal="true"
    >
    <form @submit.prevent="handleSubmit(!v$.$invalid)">
        <div class="grid formgrid m-2">
        ${formColumn()}
        </div>
    </form>
    <template #footer>
        <Button
        label="Huỷ"
        icon="pi pi-times"
        @click="closedisplayAdd${dmodel}"
        class="p-button-raised p-button-secondary"
        />
        <Button label="Cập nhật" icon="pi pi-save" @click="handleSubmit(!v$.$invalid)" />
    </template>
    </Dialog>
</template>
${style}
    `;
  return strVUE;
};
export default renderVUE;
