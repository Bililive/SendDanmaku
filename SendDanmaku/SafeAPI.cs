using LoginCenter.API;

namespace SendDanmaku
{
    internal class SafeAPI
    {
        internal SafeAPI()
        { // 尝试触发报错
            LoginCenterAPI.checkAuthorization();
        }

        internal async System.Threading.Tasks.Task<AuthorizationResult> doAuth(BilibiliDM_PluginFramework.DMPlugin plugin)
        {
            return await LoginCenterAPI.doAuthorization(plugin);
        }

        internal string send(int roomid, string msg, int color = 16777215, int mode = 1, int rnd = -1, int fontsize = 25)
        {
            return LoginCenterAPI.trySendMessage(roomid, msg, color, mode, rnd, fontsize);
        }
    }
}
