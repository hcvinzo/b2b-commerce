# Tables

## GeoLocationTypes
Desc: Geo location types like country, city, state etc
Fiels:
- Name

## GeoLocations
Desc : Hierarchical location table
Fields:
- Type
- Code -> iso-code
- Name
- Latitude { get; set; }
- Longitude { get; set; }
- Path -> 1/25/36 etc
- PathName -> Türkiye/İstanbul/Kadıköy etc
- Depth
- Metadata (json)

## Customer
Desc : Stores customer details common details.
Fields:
- Title (Ünvan) 
- TaxOffice (Vergi Dairesi)
- TaxNo (Vergi Numarası)
- EstablishmentDate Year (Kuruluş Yılı)
- Website
- Status (Durum) - Pending, Applied, Rejected, Suspended
- UserId - User related to this customer.
- DocumentUrls -> json field keeps document urls by name/type. Ex : imza sirküsü, vergi levhası etc

## CustomerContacts
Desc: Stores customer contacts informations.
Fields:
- FirstName (Adı)
- LastName (Soyadı)
- EMail (E-Posta)
- Position (Görevi)
- DateOfBirth (Doğum Tarihi)
- Gender (Cinsiyet)
- Phone (İş Telefon)
- PhoneExt (İş Telefon Dahili)
- Gsm (Mobil)

## CustomerAddresses
Desc: Stores customer addresses for billing and shipping. Customer can have multiple addresses for each type
Fields:
- Type (Adres Tipi) - Defines type of address (Billing, Shipping)
- Title (Adres Başlığı)
- FullName (Ad Soyad)
- Address (Adres)
- GeoLocation
- PostalCode (Posta Kodu)
- IsDefault -> not shown on register page on frontend. set as true
- IsActive -> not shown on register page on frontend. set as true
- AddressType -> not shown on register page on frontend. set as "Contact"
- Phone (İş Telefon)
- PhoneExt (İş Telefon Dahili)
- Gsm (Mobil)
- TaxNo
- TaxNumber

## CustomerAttributeDefinitions
Desc: Stores definitions of dynamic customer attributes. This is for extending customer definiton dynamicly. Customer have primitive attributes like text, numeric or a pre-defined value (single or multiple) or have a composite type which have sub attributes. For instance customer's deposit list have deposit type (pre-defined list), deposit amount and deposit currency
Fields:
- Code
- Name
- Type (text, integer, decimal, single selection from pre defined values, Multiple selection from predefined values, boolean, composite)
- IsRequired
- DisplayOrder
- ParentAttribute -> main attribute for composite (object) type attributes 
- IsList -> a customer can have multiple values for this attribute or not

## CustomerAttributeOptions
Desc : Stores option values for list type customer attributes
Fields:
- DisplayText
- Value
- DisplayOrder

## CustomerAttributes
Desc: Stores customer's attribute values
Fields:
- Customer
- Attribute
- Value (json)
