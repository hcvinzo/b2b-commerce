# Step 1

## İlgili Kişi (CustomerContact)
- Adı -> CustomerContact.FirstName
- Soyadı -> CustomerContact.LastName
- E-Posta -> CustomerContact.EMail
- Alternatif E-Posta -> make this email confirmation field
- Görevi -> CustomerContact.Position
- Doğum Tarihi -> CustomerContact.DateOfBirth
- Cinsiyet -> CustomerContact.Gender
- İş Telefonu -> CustomerContact.Phone
- Dahili Numara -> CustomerContact.PhoneExt
- Mobil -> CustomerContact.Gsm
Rules:
- When saving customer these information will be used to insert CustomerContact record
- Set CustomerContact.isPrimary = true 

# Step 2

## İşletme Bilgileri
- Ünvan -> Customer.Title
- Vergi Dairesi -> Customer.TaxOffice
- Vergi Numarası -> Customer.TaxNo
- Kuruluş Yılı -> Customer.EstablishmentYear
- Internet Sayfası -> Customer.Website

## İletişim
- Adres -> CustomerAddress.Address
- Ülke/Şehir/İlçe/Mahalle -> Now there is only Ülke. Add Şehir, İlçe and Mahalle and fill datas using GeoLocation according to types -> CustomerAddress.GeoLocation
- Posta Kodu -> Add "Posta Kodu" field on page -> CustomerAddress.PostalCode
- Telefon -> CustomerAddress.Phone
- Dahili -> CustomerAddress.PhoneExt
- Mobil -> CustomerAddress.Gsm
Rules:
- Add CustomerAddress row according to these informations
- CustomerAddress.Title -> "Birincil Adres"
- CustomerAddress.AddressType -> "Shipping"

## Yetkililer & Ortaklar
This is a CustomerAttribute  
Attribute Code = yetkili_ve_ortaklar (it is a composite attribute)
- Adı Soyadı -> AttributeDefinition.Code = "ad_soyad" 
- T.C. Kimlik No -> AttributeDefinition.Code = "kimlik_no" 
- Pay Oranı -> AttributeDefinition.Code = "ortaklik_payi"
Rules:
Add entered values to CustomerAttributes table

# Step 3

## İşletme Yapısı

### Personel Sayısı
- attribute with code calisan_sayisi which is a single select list attribute.
- create a component to dynamicle generate form inputs and label according to design. 

### İşletme Yapısı
- attribute
- single select list
- code -> İsletme_yapisi
- use component

### Ciro Bilgisi
- composite attribute
- code = "ciro"
- has 3 sub attribues
- use component frontend/src/components/ui/composite-attribute-input.tsx
- this is single value attribute 

### Müşteri Kitlesi
- composite attribute
- code = "musteri_kitlesi"
- has 4 sub attributes
- use component frontend/src/components/ui/composite-attribute-input.tsx
- this is single value attribute 

### Satışını Gerçekleştirdiğiniz Ürün Kategorileri
- multi select list attribute
- code = "satilan_urun_kategorileri"
- place checkboxes
- create a component for that

### Çalışmakta Olduğunuz Şirketler & Çalışma Koşulları
- composite list attribute
- code = "is_ortaklari"
- use component frontend/src/components/ui/composite-attribute-input.tsx
- multi value list

### Talep Ettiğiniz Çalışma Koşulları
- multi select list attribute
- code = "calisma_kosullari"

# Step 4

## Banka Hesap Bilgileri
- composite attribute 
- code  = "banka_hesap_bilgileri"
- multi list

## Teminatlar
- composite attribute
- code = "teminatlar"
- multi list

## Evrak & Belgeler
- files uploaded for customer
- before we store document urls in separate table mamed CustomerDocuments
- Now they will be stored in Customer.DocumentUrls field with json {"document_type":"xxx","file_type":"jpeg/png","file_url":""}