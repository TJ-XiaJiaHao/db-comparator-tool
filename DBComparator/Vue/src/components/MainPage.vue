<template>
  <div id="main">
    <h4 v-if="isNoItems">no items are different</h4>
    <table class="table table-hover">
      <thead>
      <tr>
        <th class="tb-head" v-for="(item,index) in current.header" @click="sort(index)">{{ item }}</th>
      </tr>
      </thead>
      <tbody>
      <tr v-for="(row,rowIndex) in current.body">
        <td v-for="(col,colIndex) in row">{{ col }}</td>
      </tr>
      </tbody>
    </table>
  </div>
</template>

<script>
  import bus from "../assets/eventBus"
  export default {
    name: 'DBInput',
    data () {
      return {
        isNoItems: false,
        data: {},
        tableDiff: [],
        procedureDiff: [],
        functionDiff: [],
        current: {
          header: [],
          body: [],
          sortDir: -1        // 1 means forward -1 means backward
        }
      }
    },
    methods: {
      showTable: function () {
        $("#main").animate({paddingLeft: '50px', opacity: '1'}, 500);
      },
      hideTable: function () {
        $("#main").animate({paddingLeft: '100px', opacity: '0'}, 0);
      },
      clear: function () {
        this.tableDiff = [];
        this.current.header = [];
        this.current.body = [];
      },
      showTableDiff: function () {
        this.current.header = ["NAME", "COEXIST", "BELONG", "COLUMNS", "INDEXES", "KEYS"];
        this.current.body = this.tableDiff;
        if (this.tableDiff.length == 0) {
          this.isNoItems = true;
          this.current.header = [];
        }
        else this.isNoItems = false;
      },
      showProcedureDiff: function () {
        this.current.header = ["NAME", "COEXIST", "DBNAME", "EXIST", "DBNAME", "EXIST"];
        this.current.body = this.procedureDiff;
        if (this.procedureDiff.length == 0) {
          this.isNoItems = true;
          this.current.header = [];
        }
        else this.isNoItems = false;
      },
      showFunctionDiff: function () {
        this.current.header = ["NAME", "COEXIST", "DBNAME", "EXIST", "DBNAME", "EXIST"];
        this.current.body = this.functionDiff;
        if (this.functionDiff.length == 0) {
          this.isNoItems = true;
          this.current.header = [];
        }
        else this.isNoItems = false;
      },
      sort: function (index) {
        var self = this;
        self.current.sortDir *= -1;
        this.current.body.sort(function (a, b) {
          return self.current.sortDir == 1 ? a[index] > b[index] : a[index] < b[index]
        })
      }
    },
    watch: {
      data: function (newData) {
        var self = this;
        self.clear();
        if (newData.tables == null || newData.tables == undefined)return;

        for (var i = 0; i < newData.tables.length; i++) {
          var item = newData.tables[i];
          self.tableDiff.push([item.name, item.coexist, item.dbnameIfNotCoexit, item.columns.length, item.indexes.length, item.keys.length]);
        }
        for (var i = 0; i < newData.storedProcedures.length; i++) {
          var item = newData.storedProcedures[i];
          self.procedureDiff.push([item.name, item.coexist, item.different[0].dbname, item.different[0].exist, item.different[1].dbname, item.different[1].exist]);
        }
        for (var i = 0; i < newData.functions.length; i++) {
          var item = newData.functions[i];
          self.functionDiff.push([item.name, item.coexist, item.different[0].dbname, item.different[0].exist, item.different[1].dbname, item.different[1].exist]);
        }
        self.showTableDiff();
      }
    },
    mounted() {
      var self = this;
      bus.$on("showTable", function (data) {
        self.showTable();
        self.data = data;
      })
      bus.$on("hideTable", function (data) {
        this.data = {}
        this.clear();
        self.hideTable();
      });
      bus.$on("changeData", function (data) {
        console.log("[ EVENT ] - changeData", data);
        self.data = data;
      });
      bus.$on("showTableDiff", function (data) {
        console.log("[ EVENT ] - showTableDiff");
        self.hideTable();
        self.showTable();
        self.showTableDiff();
      });
      bus.$on("showProcedureDiff", function (data) {
        console.log("[ EVENT ] - showProcedureDiff");
        self.hideTable();
        self.showTable();
        self.showProcedureDiff();
      });
      bus.$on("showFunctionDiff", function (data) {
        console.log("[ EVENT ] - showFunctionDiff");
        self.hideTable();
        self.showTable();
        self.showFunctionDiff();
      });
    }
  }
</script>

<style scoped>
  #main {
    width: 100%;
    height: calc(100% - 50px);
    padding: 20px 50px 10px 100px;
    opacity: 0;
    text-align: center;
  }

  .tb-head {
    text-align: center;
  }
</style>