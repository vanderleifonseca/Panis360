CREATE TABLE "Audits" (
    "Id" uuid NOT NULL,
    "ActorId" uuid NOT NULL,
    "Date" timestamp with time zone NOT NULL,
    "Action" text NOT NULL,
    "TargetId" uuid NOT NULL,
    CONSTRAINT "PK_Audits" PRIMARY KEY ("Id")
);


CREATE TABLE "Products" (
    "Id" uuid NOT NULL,
    "Name" text NOT NULL,
    "Group" text NOT NULL,
    "Unit" text NOT NULL,
    "Price" numeric(18,3) NOT NULL,
    "Active" boolean NOT NULL,
    CONSTRAINT "PK_Products" PRIMARY KEY ("Id")
);


CREATE TABLE "Terms" (
    "Id" uuid NOT NULL,
    "Name" text NOT NULL,
    CONSTRAINT "PK_Terms" PRIMARY KEY ("Id")
);


CREATE TABLE "Users" (
    "Id" uuid NOT NULL,
    "Name" text NOT NULL,
    "Email" text NOT NULL,
    "PasswordHash" text NOT NULL,
    "Role" text NOT NULL,
    "Active" boolean NOT NULL,
    CONSTRAINT "PK_Users" PRIMARY KEY ("Id")
);


CREATE TABLE "Lots" (
    "Id" uuid NOT NULL,
    "ProductId" uuid NOT NULL,
    "Code" text NOT NULL,
    "ExpiresOn" date NOT NULL,
    "Quantity" numeric(18,3) NOT NULL,
    "Revision" integer NOT NULL,
    CONSTRAINT "PK_Lots" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Lots_Products_ProductId" FOREIGN KEY ("ProductId") REFERENCES "Products" ("Id") ON DELETE RESTRICT
);


CREATE TABLE "PaymentTermPart" (
    "Id" uuid NOT NULL,
    "PaymentTermId" uuid NOT NULL,
    "Days" integer NOT NULL,
    "Percent" numeric(18,3) NOT NULL,
    CONSTRAINT "PK_PaymentTermPart" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_PaymentTermPart_Terms_PaymentTermId" FOREIGN KEY ("PaymentTermId") REFERENCES "Terms" ("Id") ON DELETE CASCADE
);


CREATE TABLE "Customers" (
    "Id" uuid NOT NULL,
    "Name" text NOT NULL,
    "Document" text NOT NULL,
    "Phone" text NOT NULL,
    "Street" text NOT NULL,
    "Number" text NOT NULL,
    "Neighborhood" text NOT NULL,
    "City" text NOT NULL,
    "State" text NOT NULL,
    "ZipCode" text NOT NULL,
    "SellerId" uuid NOT NULL,
    "VisitOrder" integer NOT NULL,
    "Blocked" boolean NOT NULL,
    CONSTRAINT "PK_Customers" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Customers_Users_SellerId" FOREIGN KEY ("SellerId") REFERENCES "Users" ("Id") ON DELETE RESTRICT
);


CREATE TABLE "Sessions" (
    "Id" uuid NOT NULL,
    "TokenHash" text NOT NULL,
    "UserId" uuid NOT NULL,
    "ExpiresAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_Sessions" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Sessions_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE RESTRICT
);


CREATE TABLE "Movements" (
    "Id" uuid NOT NULL,
    "StockLotId" uuid NOT NULL,
    "SaleId" uuid,
    "ActorId" uuid,
    "Quantity" numeric(18,3) NOT NULL,
    "Date" timestamp with time zone NOT NULL,
    "Reason" text NOT NULL,
    CONSTRAINT "PK_Movements" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Movements_Lots_StockLotId" FOREIGN KEY ("StockLotId") REFERENCES "Lots" ("Id") ON DELETE RESTRICT
);


CREATE TABLE "Orders" (
    "Id" uuid NOT NULL,
    "ClientRequestId" uuid NOT NULL,
    "RequestHash" text NOT NULL,
    "CustomerId" uuid NOT NULL,
    "SellerId" uuid NOT NULL,
    "PaymentTermId" uuid NOT NULL,
    "PaymentMethod" text NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "Status" text NOT NULL,
    "Revision" integer NOT NULL,
    "CutReason" text NOT NULL,
    CONSTRAINT "PK_Orders" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Orders_Customers_CustomerId" FOREIGN KEY ("CustomerId") REFERENCES "Customers" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_Orders_Terms_PaymentTermId" FOREIGN KEY ("PaymentTermId") REFERENCES "Terms" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_Orders_Users_SellerId" FOREIGN KEY ("SellerId") REFERENCES "Users" ("Id") ON DELETE RESTRICT
);


CREATE TABLE "Visits" (
    "Id" uuid NOT NULL,
    "CustomerId" uuid NOT NULL,
    "SellerId" uuid NOT NULL,
    "Date" timestamp with time zone NOT NULL,
    "Note" text NOT NULL,
    CONSTRAINT "PK_Visits" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Visits_Customers_CustomerId" FOREIGN KEY ("CustomerId") REFERENCES "Customers" ("Id") ON DELETE RESTRICT
);


CREATE TABLE "OrderItem" (
    "Id" uuid NOT NULL,
    "OrderId" uuid NOT NULL,
    "ProductId" uuid NOT NULL,
    "ProductName" text NOT NULL,
    "Requested" numeric(18,3) NOT NULL,
    "Accepted" numeric(18,3) NOT NULL,
    "UnitPrice" numeric(18,3) NOT NULL,
    CONSTRAINT "PK_OrderItem" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_OrderItem_Orders_OrderId" FOREIGN KEY ("OrderId") REFERENCES "Orders" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_OrderItem_Products_ProductId" FOREIGN KEY ("ProductId") REFERENCES "Products" ("Id") ON DELETE RESTRICT
);


CREATE TABLE "Sales" (
    "Id" uuid NOT NULL,
    "OrderId" uuid NOT NULL,
    "CustomerId" uuid NOT NULL,
    "Date" date NOT NULL,
    "Total" numeric(18,3) NOT NULL,
    CONSTRAINT "PK_Sales" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Sales_Orders_OrderId" FOREIGN KEY ("OrderId") REFERENCES "Orders" ("Id") ON DELETE RESTRICT
);


CREATE TABLE "Receivables" (
    "Id" uuid NOT NULL,
    "SaleId" uuid NOT NULL,
    "CustomerId" uuid NOT NULL,
    "Number" integer NOT NULL,
    "DueDate" date NOT NULL,
    "Amount" numeric(18,3) NOT NULL,
    "Paid" numeric(18,3) NOT NULL,
    "Revision" integer NOT NULL,
    "PaymentMethod" text NOT NULL,
    CONSTRAINT "PK_Receivables" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Receivables_Sales_SaleId" FOREIGN KEY ("SaleId") REFERENCES "Sales" ("Id") ON DELETE RESTRICT
);


CREATE TABLE "Receipts" (
    "Id" uuid NOT NULL,
    "ReceivableId" uuid NOT NULL,
    "RequestId" uuid NOT NULL,
    "Amount" numeric(18,3) NOT NULL,
    "Date" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_Receipts" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Receipts_Receivables_ReceivableId" FOREIGN KEY ("ReceivableId") REFERENCES "Receivables" ("Id") ON DELETE RESTRICT
);


CREATE UNIQUE INDEX "IX_Customers_Document" ON "Customers" ("Document");


CREATE INDEX "IX_Customers_SellerId" ON "Customers" ("SellerId");


CREATE UNIQUE INDEX "IX_Lots_ProductId_Code" ON "Lots" ("ProductId", "Code");


CREATE INDEX "IX_Movements_StockLotId" ON "Movements" ("StockLotId");


CREATE INDEX "IX_OrderItem_OrderId" ON "OrderItem" ("OrderId");


CREATE INDEX "IX_OrderItem_ProductId" ON "OrderItem" ("ProductId");


CREATE UNIQUE INDEX "IX_Orders_ClientRequestId" ON "Orders" ("ClientRequestId");


CREATE INDEX "IX_Orders_CustomerId" ON "Orders" ("CustomerId");


CREATE INDEX "IX_Orders_PaymentTermId" ON "Orders" ("PaymentTermId");


CREATE INDEX "IX_Orders_SellerId" ON "Orders" ("SellerId");


CREATE INDEX "IX_PaymentTermPart_PaymentTermId" ON "PaymentTermPart" ("PaymentTermId");


CREATE INDEX "IX_Receipts_ReceivableId" ON "Receipts" ("ReceivableId");


CREATE UNIQUE INDEX "IX_Receipts_RequestId" ON "Receipts" ("RequestId");


CREATE INDEX "IX_Receivables_SaleId" ON "Receivables" ("SaleId");


CREATE UNIQUE INDEX "IX_Sales_OrderId" ON "Sales" ("OrderId");


CREATE UNIQUE INDEX "IX_Sessions_TokenHash" ON "Sessions" ("TokenHash");


CREATE INDEX "IX_Sessions_UserId" ON "Sessions" ("UserId");


CREATE UNIQUE INDEX "IX_Users_Email" ON "Users" ("Email");


CREATE INDEX "IX_Visits_CustomerId" ON "Visits" ("CustomerId");


