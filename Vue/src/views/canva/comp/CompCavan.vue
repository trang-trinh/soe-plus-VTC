<script setup>
import { ref, inject, defineProps, onMounted, onUnmounted } from "vue";
import { fabric } from "fabric";
import "fabric-history";
const emitter = inject("emitter");
const swal = inject("$swal");
const axios = inject("axios"); // inject axios
const props = defineProps({
  id: String,
});
let canvasInstances = [];
let idxcanvas = 0;
let canvasactive;
function init(sli, i) {
  let canvasWidth = (sli && sli.stage.width) || 1920;
  let canvasHeight = (sli && sli.stage.height) || 1080;
  let containerWidth = document.getElementById("canva-view").offsetWidth - 20;
  let containerHeight = document.getElementById("canva-view").offsetHeight - 20;
  let scaleRatio = Math.min(containerWidth / canvasWidth, containerHeight / canvasHeight);
  var content = document.getElementById("canva-view");
  var newcanvas = document.createElement("canvas");
  newcanvas.style.margin = "10px";
  content.appendChild(newcanvas);
  let canvas = new fabric.Canvas(newcanvas, {
    width: canvasWidth,
    height: canvasHeight,
    backgroundColor: "#fff",
  });
  canvas.on("mouse:down", (event) => {
    if (event.button === 1) {
      console.log("left click");
    }
    if (event.button === 2) {
      console.log("middle click");
    }
    if (event.button === 3) {
      console.log("right click");
    }
    idxcanvas = i;
    canvasactive = canvas;
  });
  canvas.on({
    "object:selected": () => {
      idxcanvas = i;
      canvasactive = canvas;
    },
  });
  canvas.setDimensions({
    width: canvas.getWidth() * scaleRatio,
    height: canvas.getHeight() * scaleRatio,
  });
  canvas.setZoom(scaleRatio);
  //
  let zindex = 0;
  let addText = (it) => {
    //console.log(it);
    zindex += 1;
    var text = new fabric.Textbox(it.attrs.text, {
      fontSize: it.attrs.fontSize,
      width: it.attrs.width,
      fontWeight: it.attrs.fontStyle,
      left: it.attrs.x,
      top: it.attrs.y,
      fill: it.attrs.fill,
    });
    canvas.add(text);
    canvas.bringForward(text);
  };
  let parseGroup = (it) => {
    it.children
      .filter((x) => x.className == "Text")
      .forEach((itext) => {
        addText(itext);
      });
    it.children
      .filter((x) => x.className == "Group")
      .forEach((itext) => {
        parseGroup(itext);
      });
  };
  if (sli) {
    sli.stage.children[1].children.forEach((img) => {
      if (img.className == "Image") {
        //console.log(img);
        fabric.Image.fromURL(
          img.attrs.src &&
            (img.attrs.src.includes("app.myschool.com.vn") ||
              img.attrs.src.includes("base64"))
            ? img.attrs.src
            : `https://app.myschool.com.vn/${img.attrs.src}`,
          function (oImg) {
            if (img.attrs.x) {
              oImg.set({
                id: img.attrs.id,
                left: img.attrs.x,
                top: img.attrs.y,
              });
              if (img.attrs.width) oImg.scaleToWidth(img.attrs.width);
              if (img.attrs.height) oImg.scaleToHeight(img.attrs.height);
            } else {
              oImg.set({
                id: img.attrs.id,
                width: canvas.getWidth(),
                height: canvas.getHeight(),
                originX: "left",
                scaleX: canvas.getWidth() / oImg.width, //new update
                scaleY: canvas.getHeight() / oImg.height,
              });
            }
            canvas.add(oImg);
          }
        );
      } else if (img.className == "Group") {
        parseGroup(img);
      } else if (img.className == "Text") {
        addText(img);
      }
    });
  }
  canvasInstances.push(canvas);
  canvasactive = canvas;
}
function addCanvaImg(img) {
  fabric.Image.fromURL(img.Duongdan, function (oImg) {
    oImg.scaleToWidth(400);
    canvasactive.add(oImg);
  });
}
function addCanvaText(t) {
  var text = new fabric.Textbox("Tiêu đề", {
    fontSize: t == 1 ? 60 : t == 2 ? 40 : 20,
    fontWeight: t != 3 ? "bold" : "normal",
    left: 200,
    top: 200,
  });
  canvasactive.add(text);
}
const sendToBack = () => {
  var myObject = canvasactive.getActiveObject();
  canvasactive.sendToBack(myObject);
  canvasactive.discardActiveObject();
  canvasactive.renderAll();
};
const bringToFront = () => {
  var myObject = canvasactive.getActiveObject();
  canvasactive.bringToFront(myObject);
  canvasactive.discardActiveObject();
  canvasactive.renderAll();
};
const undo = () => {
  canvasactive.undo();
};
const redo = () => {
  canvasactive.redo();
};
const changeColor = (color) => {
  opition.textColor = color.hex;
  let obj = canvasactive.getActiveObject();
  obj.setSelectionStyles({ fill: color.hex });
  canvasactive.renderAll();
  opColor.value.hide();
};
const changeBGColor = (color) => {
  opition.textBackgroundColor = color.hex;
  let obj = canvasactive.getActiveObject();
  obj.setSelectionStyles({ textBackgroundColor: color.hex });
  canvasactive.renderAll();
  opBGColor.value.hide();
};
const setAlign = (align) => {
  opition.textAlign = align;
  let obj = canvasactive.getActiveObject();
  obj.textAlign = align;
  canvasactive.renderAll();
};

const eventKey = (event) => {
  if (event.ctrlKey) {
    switch (event.code) {
      case "KeyZ":
        undo();
        break;
      case "KeyY":
        redo();
        break;
      default:
        break;
    }
  }
};
onMounted(() => {
  init();
  emitter.on("clickMenuItem", (obj) => {
    switch (obj.type) {
      case "img":
        addCanvaImg(obj.data);
        break;
      case "itext":
        addCanvaText(obj.data);
        break;
      default:
        break;
    }
  });
  return {};
});
const opition = ref({});
const opBGColor = ref();
const opColor = ref();
const toggleColor = (event) => {
  opColor.value.toggle(event);
};
const toggleBGColor = (event) => {
  opBGColor.value.toggle(event);
};
const showDGPath = ref(false);
const jsonpath = ref(
  "https://app.myschool.com.vn/generalslide/makeslidenewver/c73b9967ee804d76901774f9ea04a28f/Edit"
);
const goBaiGiang = async () => {
  canvasInstances.forEach((cv) => {
    cv.dispose();
  });
  canvasInstances = [];
  if (jsonpath.value.includes("/Edit")) {
    let id = jsonpath.value
      .replaceAll("https://app.myschool.com.vn/generalslide/makeslidenewver/", "")
      .replaceAll("/Edit", "");
    jsonpath.value = `https://app.myschool.com.vn/Portals/BaiGiang/administrator/${id}/${id}_canva.json`;
  }
  showDGPath.value = false;
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  let rp = await axios.get("http://localhost:8091/" + encodeURIComponent(jsonpath.value));
  swal.close();
  if (rp.status == 200) {
    let objSlide = JSON.parse(rp.data.html);
    objSlide.forEach((sli, i) => {
      if (sli.stage) {
        sli.stage = JSON.parse(sli.stage);
      }
      init(sli, i);
    });
    //console.log(objSlide);
  }
};
const saveCanvas = () => {
  console.log(canvasactive.toJSON());
};
onUnmounted(() => {
  emitter.off("clickMenuItem");
});
</script>
<template>
  <div class="flex h-full w-full flex-column flex-grow-1">
    <Toolbar>
      <template #start>
        <Button
          v-tooltip.bottom="'Đổi màu chữ'"
          class="p-button-sm p-button-outlined p-button-secondary mr-1"
          :style="{
            backgroundColor: opition.textColor,
            color: opition.textColor ? 'transparent' : '#333',
            border: '1px solid #ccc',
          }"
          type="button"
          icon="pi pi-palette"
          @click="toggleColor"
        />
        <Button
          v-tooltip.bottom="'Đổi màu nền'"
          class="p-button-sm p-button-outlined p-button-secondary mr-1"
          :style="{
            backgroundColor: opition.textBackgroundColor,
            color: opition.textBackgroundColor ? 'transparent' : '#333',
            border: '1px solid #ccc',
          }"
          type="button"
          icon="pi pi-palette"
          @click="toggleBGColor"
        />
        <OverlayPanel ref="opColor">
          <ColorPicker theme="dark" @changeColor="changeColor" :sucker-hide="true" />
        </OverlayPanel>
        <OverlayPanel ref="opBGColor">
          <ColorPicker theme="dark" @changeColor="changeBGColor" :sucker-hide="true" />
        </OverlayPanel>
        <Button
          v-tooltip.bottom="'center'"
          class="p-button-sm p-button-outlined p-button-secondary mr-1"
          type="button"
          icon="pi pi-align-center"
          @click="setAlign('center')"
        />
        <Button
          v-tooltip.bottom="'justify'"
          class="p-button-sm p-button-outlined p-button-secondary mr-1"
          type="button"
          icon="pi pi-align-justify"
          @click="setAlign('justify')"
        />
        <Button
          v-tooltip.bottom="'left'"
          class="p-button-sm p-button-outlined p-button-secondary mr-1"
          type="button"
          icon="pi pi-align-left"
          @click="setAlign('left')"
        />
        <Button
          v-tooltip.bottom="'right'"
          class="p-button-sm p-button-outlined p-button-secondary mr-1"
          type="button"
          icon="pi pi-align-right"
          @click="setAlign('right')"
        />
        <Button
          v-tooltip.bottom="'Về sau'"
          class="p-button-sm p-button-outlined p-button-secondary mr-1"
          type="button"
          icon="pi pi-tablet"
          @click="sendToBack"
        />
        <Button
          v-tooltip.bottom="'lên trước'"
          class="p-button-sm p-button-outlined p-button-secondary mr-1"
          type="button"
          icon="pi pi-wallet"
          @click="bringToFront"
        />
      </template>

      <template #end>
        <Button
          v-tooltip.bottom="'Convert bài giảng'"
          class="p-button-sm p-button-outlined p-button-secondary mr-1"
          type="button"
          icon="pi pi-cog"
          @click="showDGPath = true"
        />
        <Button
          v-tooltip.left="'Lưu bài giảng'"
          class="p-button-sm p-button-outlined p-button-info"
          type="button"
          icon="pi pi-save"
          @click="saveCanvas"
        />
      </template>
    </Toolbar>

    <div id="canva-view" @keyup="eventKey" tabindex="0"></div>
  </div>
  <Dialog
    header="Chọn link bài giảng"
    v-model:visible="showDGPath"
    :style="{ width: '480px', zIndex: 2 }"
    :maximizable="true"
    :autoZIndex="false"
    :modal="true"
  >
    <form @submit.prevent="goBaiGiang">
      <div class="grid formgrid m-2">
        <div class="field col-12 md:col-12">
          <Textarea
            rows="6"
            spellcheck="false"
            class="col-12"
            v-model="jsonpath"
          ></Textarea>
        </div>
      </div>
    </form>
    <template #footer>
      <Button
        label="Huỷ"
        icon="pi pi-times"
        @click="showDGPath = false"
        class="p-button-raised p-button-secondary"
      />
      <Button label="Cập nhật" icon="pi pi-save" @click="goBaiGiang" />
    </template>
  </Dialog>
</template>
<style scoped>
#canva-view {
  flex: 1;
  overflow-y: auto;
}
</style>
