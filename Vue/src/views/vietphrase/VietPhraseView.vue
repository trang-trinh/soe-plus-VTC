<script setup>
import { ref, inject, onMounted } from "vue";
import { useToast } from "vue-toastification";
const toast = useToast();
const swal = inject("$swal");
var myWorker = new Worker("../../src/worker/reader.js");
myWorker.onmessage = function (e) {
  switch (e.data.type) {
    case "file":
      readFile(e);
      break;
    case "translate":
      renderData(e);
      break;
		default:
			break;
  }
};
const renderData = (e) => {
  switch (e.data.tran) {
    case "HV":
      hv.value = e.data.data;
      break;
    case "VP":
      vp.value = e.data.data;
      break;
		default:
			break;
  }
  toast.success(`Dịch thành công!`);
  swal.close();
};
const readFile = (e) => {
  switch (e.data.name) {
    case "ChinesePhienAmWords.txt":
      disthv = e.data.data;
      break;
    case "Names.txt":
      distname = e.data.data;
      break;
    case "VietPhrase.txt":
      distvp = e.data.data;
      break;
		default:
			break;
  }
  toast.success(`Tải dữ liệu ${e.data.name} thành công!`);
};
const tt = ref(
  `第1章 野鸡蛋
2022-01-28 作者： 仟仟梦梦
  第1章 野鸡蛋

  李氏弯着笨重的身体割草，见到不远处有一窝野鸡蛋，眼露惊喜脚步小心的在坡上走，行动不便是因为她怀孩子九个月了，眼看就要生，坐月子的时候不能没有柴火供应家里做饭。

  这是她婆婆这样吩咐的，依然记得早上婆婆去同村大姑那里帮忙照顾孩子，因为大姑生了一个儿子。

  “你这个专门生赔钱货的玩意，哪里有那么娇贵？家里没有柴火，必须要每天割两回回来。”

  李氏只能乖乖的，在快要临盆的时候，还要到山上去割草。

  她割草发现草丛有野山鸡蛋，见到野鸡蛋，想到家中面黄肌瘦的四个孩子，伸手去捡野鸡蛋，经过一处地方有一个黄蜂窝，“嗡嗡嗡”

  “啊！”

  李氏条件反射的，用她头上的草帽去扇黄峰，她的动作缓慢，没那些小虫子的动作快，脸上和手上都被针了。

  被针依然把野鸡蛋小心的更在布袋里，还把围着她嗡嗡叫的黄蜂窝摘掉，缓慢地走出这处山坡。

  感觉到脸上和手上热辣辣的疼，还有黄蜂追蛰针。

  李氏这才趴在野草里，在此休息，黄蜂逐渐飞走。

  她感觉到脸又疼又热辣辣的，还有双手肿的不听使唤，抬头看向天空，肿的只有一条缝眼朦胧看太阳升上半空，捆起来两大捆的草，用棒棒挑起七八十斤的柴草。

  脚步一脚高一脚低缓慢的走下山，走路一里回到村里，遇见一个从菜地里出来的婆婆。

  这位婆婆老眼昏花瞧见了李氏：“宏基家的，你这么大的肚子可要小心啊！宏基也真是的，他做木工有木碎。为何还要你这么大个肚子上山割草，要是摔了怎么办？”

  李氏每天都听到村人同情说这样的语言，心里想的也和这些人想的一样，只是她懦弱惯了，又是生了几个女娃娃，夫君家两代单传，到了她这一代如果没能生男娃……

  “三婆，我没事……”

  “哎呀，你的脸怎么啦？还有你双手。”三婆走近了才知道，李氏肿得像猪头的脸，露出的双手肿得肥大。

  “三婆，我没事，刚才被黄蜂丁了一下。”

  “宏基家的，你可要小心啦！那是都有毒的，不知道会不会影响孩子。”

  “这……”李氏听了三婆的话，这才惊悚后怕。

  “有没有把那个黄蜂胎摘了？摘了这个就能解毒了。”

  “有，我这就回家煮。”

  李氏盘发木簪子歪了掉下来一些凌乱的头发，臃肿的脸上一滴滴的汗滴下来，依然一脚高一脚低头走路。

  “猪头……”

  “咱们打猪头……”

  路边玩的熊孩子们拿起石头去打李氏，这些石头不是很大，李氏护着肚子，没有责斥那些熊孩子。

  “娘，你们别打我娘……”八岁大大丫提着一个竹筐，背上背着一个一岁女娃，手里牵着一个三岁女娃，她的后面跟着一个五岁的女娃也提着一个小竹筐，她们是去割猪草。

  “大丫别过来，石头会打得很疼。”李氏护女之心，四个女娃娃都在。

  “你们都是坏蛋……别打我娘……”大丫哭喊着去骂那些人，还要帮娘躲过这些熊孩子的扔石头。

  “呀！原来是生赔钱货的娘，今天变成了猪头，哈哈……”

  一个流着鼻涕，衣服上很多的补丁，袖子很短，裤子也很短，显然是不合身。

  其他的熊孩子也哈哈大笑着。

  “狗蛋你又调皮了。”一位壮汉子挑着柴从山里回来，训斥那些孩子。

  这群熊孩子吓跑。

  大丫哭着对李氏说道：“娘，你疼不疼？”

  “呜呜呜，娘……”

  大丫背上的娃娃，还有另外两个妹妹也跟着哭。

  “娘没事，咱们先回家。”

  李氏经过刚才一顿折腾，感觉肚子往下坠。

  “哭哭哭，没用的玩意，还不快点回家煮饭。”婆婆赖氏帮大女儿照顾娃娃，女儿去地里忙活回来，让她回家里吃饭，在远处听到她们母女五人哭声一顿烦躁。

  “赖氏，你的媳妇肚子这么大，你也不接柴草，宏基媳妇，你的脸怎么啦？肿成这样，来，我来帮你。”

  邻居家的李婆婆手里拿着一筐菜，他们这附近只有几家人是邻居，很同情李氏。

  “呸，你个李氏，媳妇做事不是应该的吗？关你什么事？谁让你做好人？”

  赖氏的三角眼一瞪，在地上跺跺脚，吐了一口口水，发现了李氏挂在手上的布袋子，眼睛转了转一把抢了布袋快速地回家去。

  “婆婆……”

  李氏急得要掉眼泪，婆婆拿了这个袋子，她冒着危险捡的野鸡蛋，黄蜂胎……

  “宏基家的，别急……先回家去。”

  李婆婆接了李氏的担挑手里提着她的菜篮，并不知道李氏为何对布袋子那么重视。

  李氏没管要下坠的肚子，快走几步想要追上赖氏。

  “娘，慢点走。”

  赖氏已经进了一栋只有三个房间，一个厨房的泥瓦房的院子大门，院子的另一边是茅草搭建的草棚，有一对父子正在那里做家具。

  她推门进来，正在做家具的那对父子抬头看了一眼，又继续的做家具。

  从房子里一个房间走出两个女孩，她们分别是十三岁和十五岁，这是赖氏的两个女儿，十五岁的这个女儿已经订了亲，快要嫁人在做嫁妆，另外一个跟着在家呆着。

  “娘，你带回来什么好东西？我们饿了。”

  赖氏被两个女儿一左一右抱着胳膊，无奈的把手里袋子打开。

  “哇，野鸡蛋。”

  “里面还有黄蜂胎，娘，我要炒着吃的。”

  两姐妹说到这里都感觉到口水流，拿着布袋紧跟赖氏一起回了里面的房间。

  李氏进门刚好见到两个小姑子，婆婆赖氏把布袋收起来了。

  李氏眼泪在只有一条缝的眼睛里转着，双腿呆在原地。

  “娘……”大丫带着妹妹进来，把猪菜筐放在地上，扶着母亲让出道路。

  正在做家具的那对父子，抬头看了一眼又继续的做家具。

  李婆婆把柴草挑进来，见到那一对无动于衷的父子，轻轻地把柴草放在院子，眼神责怪的看着那一对父子说道：
  “大伯，宏基……”

  听到隔壁大婶的声音，那一对父子又抬起头，做父亲的停下了手中的木活。

  “弟妹，儿媳妇也真是的，怎么能让你帮忙挑呢？”

  宏基发现了妻子肿的脸蛋，快步走过来说道：“孩子他娘，你怎么啦？”

  　新文求支持，留痕

  　　
  　
  (本章完)

`
);
const hv = ref("");
const vp = ref("");
let disthv = {};
let distname = {};
let distvp = {};
const loadTextFromFile = (ev) => {
  const file = ev.target.files[0];
  myWorker.postMessage({ type: "file", file: file });
};
const translate = () => {
  swal.fire({
    width: 110,
    didOpen: () => {
      swal.showLoading();
    },
  });
  myWorker.postMessage({
    type: "translate",
    tt: tt.value,
    tran: "HV",
    disthv: disthv,
  });
  myWorker.postMessage({
    type: "translate",
    tt: tt.value,
    tran: "VP",
    distname: distname,
    disthv: disthv,
    distvp: distvp,
  });
};
</script>
<template>
  <div class="main-layout flex-grow-1 p-2">
    <Button label="Chọn file">
      <input type="file" @change="loadTextFromFile" />
    </Button>
    <Button label="Dịch" class="ml-2" @click="translate"></Button>
    <TabView ref="tabview">
      <TabPanel header="TT">
        <ScrollPanel
          v-html="tt.replaceAll('\n', '<br>')"
          contenteditable="true"
          spellcheck="false"
          class="div-editor"
        >
        </ScrollPanel>
      </TabPanel>
      <TabPanel header="HV">
        <ScrollPanel
          v-html="hv"
          contenteditable="true"
          spellcheck="false"
          class="div-editor"
        >
        </ScrollPanel>
      </TabPanel>
      <TabPanel header="VP">
        <ScrollPanel
          v-html="vp"
          contenteditable="true"
          spellcheck="false"
          class="div-editor"
        >
        </ScrollPanel>
      </TabPanel>
    </TabView>
  </div>
</template>

<style scoped>
::-webkit-file-upload-button {
  border: none;
  background-color: transparent;
  color: transparent;
  width: 0px;
}
.div-editor {
  height: calc(100vh - 185px);
  overflow-y: auto;
}
</style>
