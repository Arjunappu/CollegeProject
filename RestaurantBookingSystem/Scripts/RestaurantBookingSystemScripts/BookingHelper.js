/*
**********************************
Utility Array Extention methods
**********************************
*/
Array.prototype.firstOrDefault = function (defaultvalue) {
    if (this[0] === undefined) {
        if (defaultvalue === undefined)
            throw new TypeError ('missing argument 0 when calling function firstOrDefault');
        else {
            return defaultvalue;
        }
    }
    return this[0];
};

Array.prototype.lastOrDefault = function (defaultvalue) {
    if (this[this.length -1] == undefined) {
        if (defaultvalue == undefined)
            throw new TypeError('missing argument 0 when calling function lastOrDefault');
        else {
            return defaultvalue;
        }
    }
    return this[this.length -1];
};

/*
**********************************
A Generic Dictionary
**********************************
*/
function Dictionary(dictionaryItems, dictionatyKeyType, dictionaryValueType) {
    this.KeyType = undefined;
    this.ValueType = undefined;
    this.KeyValuePairs = new Array();
    
    this.clear = function () {
        this.KeyValuePairs.length = 0;
        this.KeyValuePairs = new Array();
    };

    this.atIndex = function (index) {
        index = parseInt(index);
        if ($.isNaN(index))
            throw new TypeError("invalid argument type when calling function atIndex");
        return this.KeyValuePairs[index];
    };

    this.indexOf = function (key) {
        if (key == undefined)
            return -1;
        if (key instanceof KeyValuePair)
            return this.KeyValuePairs.indexOf(key);
        else {
            var length = this.KeyValuePairs.length;
            if (!length)
                return -1;
            for (var i = 0; i < length; ++i) {
                if (this.KeyValuePairs[i].Key === key)
                    return i;
            }
            return -1;
        }
    };

    this.get = function (key) {
        var length = this.KeyValuePairs.length;
        if (!length || !key)
            return null;
        for (var i = 0; i < length; ++i) {
            if (this.KeyValuePairs[i].Key === key)
                return this.KeyValuePairs[i];
        }
        return null;
    };

    this.pop = function () {
        return this.KeyValuePairs.pop();
    };

    this.push = function (keyvaluepair) {
        if (keyvaluepair instanceof KeyValuePair)
            return this.add(keyvaluepair);
        return false;
    };

    this.add = function () {
        if (arguments == undefined || arguments.length < 1 || arguments[0] == undefined)
            throw new TypeError('invalid or missing arguments when calling function add');
        var i = 0;
        if (arguments[0] instanceof Dictionary) {
            for (i = 0; i < arguments[0].KeyValuePairs.length; ++i)
                this.KeyValuePairs[i] = arguments[0].KeyValuePairs[i];
            return true;
        }
        if (arguments[0] instanceof Array && !$.isPlainObject(arguments[0])) {
            if (arguments[0][0] instanceof KeyValuePair) {
                for (i = 0; i < arguments[0].length; ++i)
                    this.add(arguments[0][i]);
                return true;
            }
            if ($.isPlainObject(arguments[0][0])) {
                for (i = 0; i < arguments[0].length; ++i)
                    this.add(arguments[0][i]);
                return true;
            }
            return false;
        }
        if ($.isPlainObject(arguments[0])) {
            if ('Key' in arguments[0] && 'Value' in arguments[0]) {
                this.add(new KeyValuePair(arguments[0]['Key'], arguments[0]['Value']));
                return true;
            }
            return false;
        }
        if (arguments.length === 2) {
            this.add(new KeyValuePair(arguments[0], arguments[1]));
            return true;
        }
        if (arguments[0] instanceof KeyValuePair) {
            var tmpkey = arguments[0].Key;
            var tmpValue = arguments[0].Value;

            if (typeof this.KeyType === 'function')
                tmpkey = new this.KeyType(tmpkey);

            if (this.get(tmpkey) !== null)
                throw new Error('Duplicate keys are not allowed and an item already exist with given key: ' + tmpkey);

            if (typeof this.ValueType === 'function')
                tmpValue = new this.ValueType(tmpValue);

            this.KeyValuePairs.push(new KeyValuePair(tmpkey, tmpValue));
            return true;
        }
        return false;
    };

    this.remove = function (key) {
        var foundindex = this.indexOf(key);
        if (foundindex !== -1) {
            var tmpitem = this.atIndex(foundindex);
            delete this.KeyValuePairs[foundindex];
            return tmpitem;
        }
        return null;
    };

    //check for any arguments and try initializing with those arguments
    if (arguments == undefined || arguments.length < 1)
        return this;
    var _i_ = 0;
    var __add__ = undefined;
    if (typeof arguments[_i_] instanceof Dictionary) {
        __add__ = arguments[_i_];
        this.KeyType = arguments[_i_];
        this.ValueType = arguments[_i_++];
    }
    if (typeof arguments[_i_] !== 'function')
        __add__ = arguments[_i_++];
    if (typeof arguments[_i_] === 'function' && typeof arguments[_i_ + 1] === 'function') {
        this.KeyType = arguments[_i_++];
        this.ValueType = arguments[_i_++];
    }
    if (typeof arguments[_i_] === 'function')
        this.KeyType = arguments[_i_++];
    if (__add__ !== undefined)
        this.add(__add__);
    delete __add__;
    delete _i_;

    return this;
};

/*
**********************************
A Generic KeyValue Pair
**********************************
*/
function KeyValuePair(key, value) {
    if (key == undefined)
        throw new TypeError('missing argument key when calling constructor for KeyValuePair');
    if (value == undefined)
        throw new TypeError('missing argument value when calling constructor for KeyValuePair');

    this.Key = key;
    this.Value = value;
};

/*
**********************************
A Generic List
**********************************
*/
function List(listItems, listItemsType) {
    this.ListType = undefined;
    this.Items = new Array();

    this.clear = function () {
        this.Items.length = 0;
        this.Items = new Array();
    };

    this.atIndex = function (index) {
        index = parseInt(index);
        if ($.isNaN(index))
            throw new TypeError("invalid argument type when calling function atIndex");
        return this.Items[index];
    };

    this.indexOf = function (item) {
        if (item == undefined)
            return -1;
        var length = this.Items.length;
        if (!length)
            return -1;
        return this.Items.indexOf(item);
    };
    
    this.add = function () {
        if (arguments == undefined || arguments.length < 1 || arguments[0] == undefined)
            throw new TypeError('invalid or missing arguments when calling function add');
        var i = 0;
        if (arguments[0] instanceof List) {
            for (i = 0; i < arguments[0].Items.length; ++i)
                this.Items[i] = arguments[0].Items[i];
            return true;
        }
        if ( arguments[0].length !== undefined && arguments[0] instanceof Array) {
            for (i = 0; i < arguments[0].length; ++i)
                this.add(arguments[0][i]);
            return true;
        }
        var tmpitem = arguments[0];

        if (typeof this.ListType === 'function')
            tmpitem = new this.ListType(tmpitem);
        this.Items.push(tmpitem);
        return true;
    };

    this.push = function () {
        var result = false;
        var i;
        for (i = 0; i < arguments.length; ++i)
            result =  this.add(arguments[i]);
        return result;
    };

    this.pop = function () {
        return this.Items.pop();
    };

    //check for any arguments and try initializing with those arguments
    if (arguments == undefined || arguments.length < 1)
        return this;
    var _i_ = 0;
    var __add__ = undefined;
    if (typeof arguments[_i_] instanceof List) {
        __add__ = arguments[_i_];
        this.ListType = arguments[_i_++].ListType;
    }
    if (typeof arguments[_i_] !== 'function')
        __add__ = arguments[_i_++];
    if (typeof arguments[_i_] === 'function')
        this.ListType = arguments[_i_++];
    if (__add__ !== undefined)
        this.add(__add__);
    delete __add__;
    delete _i_;

    return this;
};

/*
**********************************
A Restaurant Booking
**********************************
*/
function Booking() {
    this.Tables = new Array();
    this.MenuItems = new Array();
};

/*
**********************************
A restaurant Table 
**********************************
*/
function RestaurantTable() {
    if (arguments == undefined || arguments.length !== 1 || typeof arguments[0] !== 'object' || typeof arguments[0].TableId  !== 'number')
        throw new Error("invalid or missing arguments value when calling constructor for RestaurantTable");
    
    var table = arguments[0];
    this.Alignment = table.Alignment;
    this.FloorPlanFileName = table.FloorPlanFileName;
    this.Position = new Point(table.Position.X, table.Position.Y);
    this.Price = table.Price;
    this.Status = table.Status;
    this.TableId = table.TableId;
    this.TableType = table.TableType;
}

RestaurantTable.RestaurantTableType = {
    'Dual': 2,
    'Quad': 4,
    'Hex': 6,
    'Oct': 8
};

RestaurantTable.RestaurantTableAlignment = {
    'Horizontal': 0,
    'Vertical': 1
};

RestaurantTable.RestaurentTableStatus = {
    'NotApplicable': -1,
    'Booked': 0,
    'Occupied': 1,
    'Vacant': 2
};

/*
**********************************
Restaurant Offer
**********************************
*/
RestaurantOffer = new Object();
RestaurantOffer.Type = {
    'DiscountAmount' : 0,
    'DiscountPercent' : 1,
    'FreeServing' : 2
};

/*
**********************************
A Point
**********************************
*/
function Point(x, y) {
    if (typeof x !== 'number' || typeof y !== 'number') {
        throw new TypeError("The type of X and Y should be of Number type");
    }
    this.X = x;
    this.Y = y;
    
    return this;
};

/*
**********************************
A RestaurantTable List
**********************************
*/
function RestaurantTableList() {
    return List.call(this, arguments[0], RestaurantTable);
};

RestaurantTableList.prototype = new List(RestaurantTable);
RestaurantTableList.prototype.constructor = RestaurantTableList;

/*
**********************************
A Method to get Restaurant Table Status for given time and Booking slots
**********************************
*/
function getTableStatusOn(url, fromdatetime, bookingslots, minutesinoneslot, callback) {
    if (!url || !fromdatetime || !bookingslots || !minutesinoneslot || typeof url !== 'string' || !(fromdatetime instanceof Date) || typeof bookingslots !== 'number' || typeof minutesinoneslot !== 'number' || typeof callback !== 'function')
        throw new TypeError("invalid or missing arguments when calling function getTableStatusOn()");

    var result = new Dictionary(Date, RestaurantTableList);
    
    var todatetime = new Date(fromdatetime.getTime() + (1000 * 60 * minutesinoneslot * bookingslots));
    $.post(url, { 'fromdatetime': fromdatetime.toISOString(), 'todatetime': todatetime.toISOString() }, function (response, status) {
        if (status == 'success') {
            $(response).each(function () {
                result.add(this.pop());
            });
            callback.call(result);
        }
        else {
            showViewNotification(false, "There was a network related error, please try again !");
        }
    }, 'json');
};

/*
**********************************
A Method to get List of Restaurant Table Status Merged for given slots
**********************************
*/
function getMergedTableStatusOf(tableSatusDictionary, startSlot, numberofSlots) {
    if (!(tableSatusDictionary instanceof Dictionary) || typeof startSlot !== 'number' || typeof numberofSlots !== 'number')
        throw new TypeError("invalid or missing arguments when calling function getMergedTableStatusOf()");
    var dict = new Dictionary(tableSatusDictionary);
    var result = new RestaurantTableList();
    var prevtable = null;
    var table = null;
    var tablelist = null;
    for (var i = startSlot; i < startSlot + numberofSlots; ++i) {
        tablelist = new RestaurantTableList(dict.KeyValuePairs[i].Value);
        for (var j = 0; j < dict.KeyValuePairs[i].Value.Items.length; ++j) {
            prevtable = tablelist.Items[j];
            table = dict.KeyValuePairs[i].Value.Items[j];
            table.Status = Math.min(prevtable.Status, table.Status);
        }
        tablelist[j] = table;
    }
    result = tablelist;
    return result;
};