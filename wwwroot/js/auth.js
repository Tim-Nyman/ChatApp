window.authHelper = {
    getCurrentUser: function () {
        const user = sessionStorage.getItem("currentUser");
        return user ? JSON.parse(user) : null;
    },
    setCurrentUser: function (user) {
        sessionStorage.setItem("currentUser", JSON.stringify(user));
    },
    clearCurrentUser: function () {
        sessionStorage.removeItem("currentUser");
    }
};