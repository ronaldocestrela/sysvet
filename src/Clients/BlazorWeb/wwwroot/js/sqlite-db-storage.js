window.sysvetDbStorage = {
    get: function () {
        return new Promise(function (resolve, reject) {
            var request = indexedDB.open('SysVetOffline', 1);
            request.onupgradeneeded = function (event) {
                event.target.result.createObjectStore('files');
            };
            request.onerror = function () { reject(request.error); };
            request.onsuccess = function (event) {
                var db = event.target.result;
                var tx = db.transaction('files', 'readonly');
                var getReq = tx.objectStore('files').get('sysvet.db');
                getReq.onerror = function () { reject(getReq.error); };
                getReq.onsuccess = function () {
                    if (!getReq.result) {
                        resolve(null);
                        return;
                    }
                    resolve(Array.from(new Uint8Array(getReq.result)));
                };
            };
        });
    },
    put: function (data) {
        return new Promise(function (resolve, reject) {
            var buffer = new Uint8Array(data);
            var request = indexedDB.open('SysVetOffline', 1);
            request.onupgradeneeded = function (event) {
                event.target.result.createObjectStore('files');
            };
            request.onerror = function () { reject(request.error); };
            request.onsuccess = function (event) {
                var db = event.target.result;
                var tx = db.transaction('files', 'readwrite');
                tx.objectStore('files').put(buffer, 'sysvet.db');
                tx.oncomplete = function () { resolve(); };
                tx.onerror = function () { reject(tx.error); };
            };
        });
    }
};
