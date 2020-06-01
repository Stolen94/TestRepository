define("ContactSectionV2", ["ConfigurationConstants"], 
function(ConfigurationConstants) {
    return {
        // Название схемы объекта раздела.
        entitySchemaName: "Contact",
        attributes: {
            // Атрибут для хранения состояния доступности кнопки.
            "ButtonEnabled": {
                "dataValueType": Terrasoft.DataValueType.BOOLEAN,
                "type": Terrasoft.ViewModelColumnType.VIRTUAL_COLUMN,
                "value": false
            }
        },
        // Методы модели представления раздела.
        methods: {
            //========================================================================================================
            showNameAgeInfo: function() 
            {
                // 
				var activeRow = this.get("ActiveRow");
                if (!activeRow) {
                    return;
                }
                
                // Получаем [Id] объекта карточки.
				var recordId = this.get("GridData").get(activeRow).get("Id");
				
				//Создание объекта запроса
				var esq = this.Ext.create("Terrasoft.EntitySchemaQuery", 
				{
    				rootSchemaName: "Contact"
				});
				
				// Добавляем колонки имени и возраста.
				esq.addColumn("Name", "zName");
				esq.addColumn("Age", "zAge");
				// Получаем одну запись из выборки по [Id] объекта карточки
				esq.getEntity(recordId, function(result) {
    			if (!result.success) 
    			{
        		// обработка ошибки, например
        		this.showInformationDialog("Ошибка запроса данных");
        		return;
    			}
    			
    			//Отображение сообщения================================================================================
    			//=====================================================================================================
    			var message = "";
    			
    			//Отображение сообщения, если задано имя и не задан возраст
    			if (result.entity.get("zAge")===0 && result.entity.get("zName")!=="") 
    			{
    			    message+=result.entity.get("zName");
    				this.showInformationDialog(message);
    				return;
    			}
    			//Отображение сообщения, если имя не заданы
    			    if (result.entity.get("zName")==="") 
    			{
    			    message+=("No name");
    				this.showInformationDialog(message);
    				return;
    			}
    			
    			//Отображение сообщения, если заданы возраст и имя
    			
    			if ((result.entity.get("zName")!=="" && result.entity.get("zAge")!==0) ) 
    			{
    			    message+=result.entity.get("zName") + ", " + result.entity.get("zAge");
    				this.showInformationDialog(message);
    				return;
    			}
    			
				}, this);
				
            },
            //========================================================================================================
            isEnabled: function() {
				return true;
            },

            
        },
        //Настройка визуализации кнопки на странице редактирования.
        diff: [
            // Метаданные для добавления на страницу пользовательской кнопки.
            {
                // Выполняется операция добавления элемента на страницу.
                "operation": "insert",
                // Мета-имя родительского контейнера, в который добавляется кнопка.
                "parentName": "CombinedModeActionButtonsCardLeftContainer",
                // Кнопка добавляется в коллекцию компонентов
                // родительского элемента.
                "propertyName": "items",
                // Мета-имя добавляемой кнопки.
                "name": "NameAgeButton",
                // Свойства, передаваемые в конструктор компонента.
                "values": {
                    // Тип добавляемого элемента — кнопка.
                    "itemType": Terrasoft.ViewItemType.BUTTON,
                    // Привязка заголовка кнопки к локализуемой строке схемы.
                    "caption": {bindTo: "Resources.Strings.ShowNameAgeInfoButtonCaption"},
                    // Привязка метода-обработчика нажатия кнопки.
                    "click": {bindTo: "showNameAgeInfo"},
                    // Стиль отображения кнопки.
                    "style": Terrasoft.controls.ButtonEnums.style.GREEN,
                    // Привязка свойства доступности кнопки.
                    "enabled": {bindTo: "isEnabled"},
                }
            }
        ]
    };
});