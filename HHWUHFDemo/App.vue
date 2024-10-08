<script>
	let main = plus.android.runtimeMainActivity();
	// ！！！为了保证UHF正常关闭下电，延迟退出APP！！！
	if (uni.getSystemInfoSync().platform == 'android') {
		plus.runtime.quit = function() {
			setTimeout(() => {
				main.finish()
			}, 10)
		};
	}
	var hhwUHFController;
	// var socketTask; 

	export default {
		globalData: {
			hhwUHFController: uni.requireNativePlugin('HL-HHWUHFController'),
			globalEvent: uni.requireNativePlugin('globalEvent'),
			dataSet: {
				area: '',
				userId: 'S2239001O'
			},
			status: '',
		},
		onLaunch: function() {
			hhwUHFController = getApp().globalData.hhwUHFController;
		},
		onShow: function() {
			// 开启插件java接口的日志打印
			hhwUHFController.setDebuggable(true, result => {
				console.log("App Show", "setDebuggable: " + result);
			});
			// 初始化超高频（UHF），上电
			var enterTime = Date.now();
			var outTime = enterTime;
			hhwUHFController.initUhf(result => {
				outTime = Date.now();
				console.log("App Show", "initUHF: " + result, "cusTime: " + (outTime - enterTime));
				uni.showToast({
					title: result,
					icon: "none",
					duration: 1000
				});
			});
		},
		onHide: function() {
			// 关闭超高频（UHF），下电
			hhwUHFController.closeUhf(result => {
				console.log("App Hide", "closeUhf: " + result);
				uni.showToast({
					title: result,
					icon: "none",
					duration: 1000
				});
			});
		},
	}
</script>

<style>
	.buttom-bar {
	  position: fixed; 
	  bottom: 0; 
	  left: 0; 
	  right: 0; 
	  background-color: #f8f8f8; 
	  padding: 10px;
	  border-top: 1px solid #ccc; 
	}
</style>
