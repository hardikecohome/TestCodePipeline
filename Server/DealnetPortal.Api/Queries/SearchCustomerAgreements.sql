DECLARE @CONTRACT  VARCHAR(10);
DECLARE @LEASECOMPANY VARCHAR(10);
DECLARE @CUST_ID VARCHAR(10);

DECLARE @CUST_FIRST_NAME VARCHAR(50);
DECLARE @CUST_LAST_NAME VARCHAR(50);
DECLARE @CUST_DATE_OF_BIRTH AS DATETIME;
DECLARE @CUST_COMPANY AS VARCHAR(10);

DECLARE @F_PAY_DATE DATETIME;
DECLARE @L_PAY_DATE DATETIME;

DECLARE @TABLE  TABLE  
(

lease_num varchar(15),
lease_app_num varchar(15),
lease_book_type varchar(10),
lease_booked_post_date datetime,
lease_sign_date datetime,
lease_start_date datetime,
lease_maturity_date datetime,
lease_term varchar(4),
lease_type_desc varchar(20),
lease_type varchar(10),
lease_cust_id_num varchar(15),
equip_type varchar(5), 
equip_type_desc varchar(30),
equip_active_flag varchar(5)

)

/*

select  lease_book_type, lease_booked_post_date, lease_sign_date, lease_start_date,lease_term,lease_type_desc,lease_type  from LPlusLeaseVW 
where lease_cust_id_num =@CUST_ID


*/
SET @CUST_FIRST_NAME = '{0}'
SET @CUST_LAST_NAME ='{1}'
SET @CUST_DATE_OF_BIRTH = '{2}'
SET @CUST_COMPANY = '08'


DECLARE SMS_CURSOR CURSOR FOR 
   
   
SELECT	 
/*
entt.date_of_birth,
ROLECOMP.comp_company_num AS cust_comp_num, 
*/
ROLECOMP.role_id AS cust_id_num
/*			,addr.addr_line1 AS cust_address1, addr.addr_line2 AS cust_address2, 
			
			addr.city AS cust_city, 
			entt.comptype_company_type AS cust_comp_type_code, comptype.descr AS cust_comp_type_desc,
			cont.title AS cust_contact_title, 
			addr.county AS cust_county, addr.country AS cust_country, 
				CASE
					WHEN cntrl.CntrlTaxModule = 1
						THEN SUBSTRING(CAST(1000000000 + CAST(loca.geo_code AS Numeric(9,0)) AS varchar(10)), 4, 3)
					WHEN cntrl.CntrlTaxModule = 0 or cntrl.CntrlTaxModule = 2			
						THEN loca.county_tax_code
				END AS cust_county_tax_code,
		
			cust.credit_line AS cust_credit_line, cust.credit_rating AS cust_credit_rating, 
			entt.db_num AS cust_db_num, entt.db_rating AS cust_db_rating, 
			entt.dba AS cust_dba_name, rolecomp.cust_delcd_code AS cust_default_delinq_code, 
			rolecomp.cust_invcd_code AS cust_default_inv_code, cust.email_addr AS cust_email_address, 
			entt.ucc_entity_type AS cust_entity_type, 

		
			entt.bus_or_ind AS cust_federal_id_indicator, entt.tax_id_number AS cust_federal_id_num,
			entt.ityp_industry_type AS cust_industry, ityp.descr AS cust_industry_desc, 
			entt.last_financial_date AS cust_last_financial_date, entt.legal_name AS cust_legal_name, 
			entt.name AS cust_name, 
			entt.fname AS customer_first_name,
	
			entt.lname AS customer_last_name,
			entt.ucc_organization_id AS cust_org_id, 
			parentrolecomp.role_id AS cust_parent_cust_id,
			
			cust.performance_status AS cust_performance_status_code,
			DataTable.descr AS cust_performance_status_descr,	

			CASE
				WHEN LEN(ISNULL(cont.mname, '')) > 0 
					THEN ISNULL(cont.fname,'') + ' ' + ISNULL(cont.mname,'') + ' ' + ISNULL(cont.lname,'') 
				WHEN LEN(ISNULL(cont.mname, '')) = 0
					THEN ISNULL(cont.fname,'') + ' ' + ISNULL(cont.lname,'')
			END AS cust_primary_contact, 

--custaux.CustYrsInBusiness AS cust_yrs_in_business, 
			addr.postal_code AS cust_zip_code
			*/
FROM Entity entt
INNER JOIN Role role ON 
	Entt.oid = role.entt_oid AND 
	role.role_type = 'CUST' 
INNER JOIN RoleCompany rolecomp ON 
	role.oid = rolecomp.role_oid
INNER JOIN Customer cust ON
	cust.oid = role.ref_oid and
	role.role_type = 'CUST'
LEFT JOIN Entity parent ON
	parent.oid = entt.parent_oid
LEFT JOIN Role parentrole ON 
	parent.oid = parentrole.entt_oid AND 
	parentrole.role_type = 'CUST' 
LEFT JOIN RoleCompany parentrolecomp ON 
	parentrole.oid = parentrolecomp.role_oid
LEFT JOIN Customer parentcust ON
	parentcust.oid = parentrole.ref_oid and
	parentrole.role_type = 'CUST'
LEFT JOIN Contact cont ON
	cont.ref_oid = cust.oid AND
	cont.ref_type = 'CUST' AND
	cont.ctex_object_type = 'CUST' AND
	cont.usage_ind = 'P'
LEFT JOIN IndustryType ityp ON
	ityp.industry_type = entt.ityp_industry_type
LEFT JOIN LeaseSalesRep slsrep ON
	slsrep.SlsrepCompanyNum = RoleComp.comp_company_num AND
	slsrep.SlsrepIdNum = RoleComp.cust_slsrep_id_num
LEFT JOIN Location loca ON
	loca.ref_oid = Cust.oid AND
	loca.ref_type = 'CUST' AND
	loca.location_code = 'DFT'
LEFT JOIN Address addr ON
	addr.oid = loca.addr_oid 
LEFT JOIN AddressXref addx ON
	addx.addr_oid = addr.oid AND
	addx.ref_type = 'LOCA' AND
	addx.usage_ind = 'P'
LEFT JOIN Location btloca ON
	btloca.ref_oid = Cust.oid AND
	btloca.ref_type = 'CUST' AND
	btloca.location_code = 'BIL'
LEFT JOIN Address btaddr ON
	btaddr.oid = btloca.addr_oid 
LEFT JOIN AddressXref btaddx ON
	btaddx.addr_oid = addr.oid AND
	btaddx.ref_type = 'LOCA' AND
	btaddx.xref_type = 'B' AND
	btaddx.usage_ind = 'S'
LEFT JOIN CompanyType comptype ON
	comptype.company_type = entt.comptype_company_type
LEFT JOIN DataTable ON 
	table_key = 'PERFORMANCE_STATUS' AND 
    data_value = cust.performance_status 
LEFT OUTER JOIN LeaseControl cntrl ON
	cntrl.CntrlCompanyNum = rolecomp.comp_company_num


where 
entt.fname  = @CUST_FIRST_NAME  and 
entt.lname  = @CUST_LAST_NAME  
--and entt.date_of_birth = @CUST_DATE_OF_BIRTH
and ROLECOMP.comp_company_num = @CUST_COMPANY 

order by cust_id_num desc
  
  OPEN SMS_CURSOR

  FETCH NEXT FROM SMS_CURSOR
  INTO @CUST_ID 

  WHILE @@FETCH_STATUS = 0
  BEGIN
  PRINT   @cust_id;

  insert into @TABLE 


select  
lease_num,
lease_app_num,
lease_book_type, 
lease_booked_post_date, 
lease_sign_date, 
lease_start_date,
lease_maturity_date,
lease_term,
lease_type_desc,
lease_type, 
lease_cust_id_num,
equip.equip_type, 
equip.equip_type_desc,
equip.equip_active_flag

   from LPlusLeaseVW  as lease
  left join [LPlusEquipmentVW]  as equip

  on lease.lease_comp_num = equip.equip_comp_num  and lease.lease_cust_id_num = equip.equip_cust_id_num
  and lease.lease_num = equip.equip_lease_num

where lease_cust_id_num = @CUST_ID
and lease.lease_term_date is null

  FETCH NEXT FROM SMS_CURSOR
  INTO @CUST_ID

  END
  
  CLOSE SMS_CURSOR
  DEALLOCATE SMS_CURSOR

 select * from @TABLE

 delete from @TABLE