# LogUtil
# How To Use
in you Global.asax.cs Application_Start Method
add the code below
protected void Application_Start()
{
	//todo other init stuff
	//start the log thread
	APILogManager.Instance.Initialize(Server.MapPath("~/config/LogConfig.xml"), Server.MapPath("~/log/"));
	APILog.Kernel.LogError("Failed to initialize log manager");
}

then exit:

protected void Application_End(object sender, EventArgs e)
{
	APILog.Kernel.LogInfo("Application_End");
	APILogManager.Instance.Exit();
}