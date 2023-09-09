<script setup lang="ts">
import { reactive, ref } from 'vue';
import axios from 'axios';
import * as signalR from '@microsoft/signalr';
interface LoginResp {
  name: string;
  token: string;
}
const myName = ref("");
const testResp = ref("");
const userInfo = reactive({
  userName: "wtf",
  password: "123456"
});

const userMessage = ref("");
const privateMessage = reactive({
  toUserName: "",
  message: ""
});

const messages = ref<string[]>([]);
const dictImportState = ref("开始导入");
const dictImportProgress = reactive({
  now: 0,
  max: 1
});

let signalRConn: signalR.HubConnection;

const startsignalRConn = async (token: string | undefined) => {
  if (signalRConn) await signalRConn.stop();
  signalRConn = new signalR.HubConnectionBuilder()
    .withUrl("http://localhost:5021/Hubs/ChatRoomHub", {
      skipNegotiation: true,
      transport: signalR.HttpTransportType.WebSockets,
      accessTokenFactory: () => token ?? localStorage.getItem("token") ?? ""
    })
    .withAutomaticReconnect()
    .build();
  try {
    await signalRConn.start();
  } catch (err) {
    alert("SignalR连接失败，" + err);
    return;
  }
  signalRConn.on("ReceivePublicMessage", (msg: string) => { // 监听服务端发送的消息
    messages.value.push(msg);
  });
  signalRConn.on("ReceicePrivateMessage", (currentUser: string, toUser: string, time: string, msg: string) => {
    messages.value.push(`${currentUser}在${time}对${toUser}私信: ${msg}`);
  });
  
  signalRConn.on("DictImportState",(state: string) => {
    dictImportState.value = state;
  });
  signalRConn.on("DictImportProgress", (now: number, max: number) => {
    dictImportProgress.max = max;
    dictImportProgress.now = now;
  });
};

const Login = async () => {
  try {
    const res = await axios.post<LoginResp>('http://localhost:5021/api/Test/Login', userInfo);
    alert("登录成功");
    localStorage.setItem("token", res.data.token);
    startsignalRConn(res.data.token);
    myName.value = res.data.name;
  } catch (error) {
    userInfo.password = "";
    userInfo.userName = "";
    alert("登录失败，" + error);
  }
};

const LoginOut = async () => {
  if (signalRConn) await signalRConn.stop(); // 停止连接
  localStorage.removeItem("token");
};

const AddUser = async () => {
  try {
    const res = await axios.post<string>('http://localhost:5021/api/SignalRDemo/AddUser', userInfo);
    alert(res.data);
  } catch (error) {
    userInfo.password = "";
    userInfo.userName = "";
    alert("添加失败，" + error);
  }
};

const testToken = async () => {
  const res = await axios.get<string>('http://localhost:5021/api/Hello', {
    headers: {
      Authorization: `Bearer ${localStorage.getItem("token") ?? ""}`
    }
  });
  testResp.value = res.data;
};

const sendMsg = async () => {
  try {
    await signalRConn.invoke("SendPublicMessage", userMessage.value); // 客户端向服务器端发送消息
    userMessage.value = "";
  } catch (error) {
    alert("无法发送消息，" + error);
  }
};
const sendPrivateMsg = async () => {
  try {
    let res = await signalRConn.invoke<string>("SendPrivateMessage", privateMessage.toUserName, privateMessage.message);
    alert(res);
    privateMessage.message = "";
    privateMessage.toUserName = "";
  } catch (error) {
    alert("无法发送消息，" + error);
  }
};

const importDict = async () => {
  try {
    await signalRConn.invoke("ImportECDict");
  } catch (error) {
    alert("导入失败，" + error);
  }
}

</script>

<template>
  <fieldset>
    <legend>
      SignalRClient1
      <span v-show="myName"> - 当前用户: {{ myName }}</span>
    </legend>
    <div>
      用户名：<input type="text" v-model="userInfo.userName">
    </div>
    <div>
      密&nbsp;&nbsp;&nbsp;码：<input type="text" v-model="userInfo.password">
    </div>
    <hr>
    <button @click="Login">登录</button>
    <button @click="LoginOut">登出</button>
    <button @click="AddUser">添加</button>
    <hr>
    <button @click="testToken">测试登录后请求</button>
    <div>{{ testResp }}</div>
    <hr>
    广播消息：<input type="text" v-model="userMessage" @keyup.enter="sendMsg" />
    <legend>消息屏</legend>
    <div>
      <ul>
        <li v-for="(msg, index) in messages" :key="index">{{ msg }}</li>
      </ul>
    </div>
    <hr>
    <legend>私聊</legend>
    <div>
      私聊对<input type="text" v-model="privateMessage.toUserName" />
      说<input type="text" v-model="privateMessage.message" @keyup.enter="sendPrivateMsg" />
    </div>
    <hr>
    <legend>导入词典</legend>
    <button @click="importDict" :disabled="dictImportState !== '开始导入'">
      {{ dictImportState }}
      <span v-show="dictImportProgress.now !==0">({{ dictImportProgress.now }}/{{ dictImportProgress.max }})</span>
    </button>
    <div>
      <progress :value="dictImportProgress.now" :max="dictImportProgress.max"></progress>
    </div>
  </fieldset>
</template>