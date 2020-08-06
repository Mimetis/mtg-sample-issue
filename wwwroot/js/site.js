// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your Javascript code.


function setMgtProvider() {
    const provider = new mgt.ProxyProvider("/api/Proxy");
    provider.login = () => window.location.href = '/Account/SignIn?redirectUri=' + window.location.href;
    provider.logout = () => window.location.href = '/MicrosoftIdentity/Account/SignOut';

    mgt.Providers.globalProvider = provider;

    console.log("State init = " + provider.state);

    // The state changed to State==2 every time. Logged in or not, it's always 2
    provider.onStateChanged((e) => {
        console.log("-- State Changed raised :");
        console.log("State changed = " + provider.state);

    })
}


function interceptMgtLogin() {
    var mgtlogin = document.getElementById('mgtlogin');

    // Theses events are raised when user click on login our logout button
    // Theyr are not raised when the user is eventually logged in or logged out

    // Should be renamed 'loginClick' and 'logoutClick'
    mgtlogin.addEventListener('loginCompleted', () => {
        console.log("-- loginCompleted raised :");
        console.log("Login Completed. This event should be raised once the user is eventually logged in.")
    });
    mgtlogin.addEventListener('logoutCompleted', () => {
        console.log("-- logoutCompleted raised :");
        console.log("Login Completed. This event should be raised once the user is eventually logged in.")
    });

    // Loading completed is correctly fired AFTER component is loaded
    mgtlogin.addEventListener('loadingCompleted', () => {
        console.log("-- loadingCompleted raised :");
        console.log("Loading Completed. This event should be raised each time the component is loaded");
    });
}


//Do not wait until the page is loaded to be sure we won't have any "clipping" visual effect
//And affect AS SOON AS POSSIBLE the user details object stored, if any
setMgtProvider();
interceptMgtLogin();

// Once page is loaded
$(function () {

});