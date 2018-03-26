SELECT distinct top 1 

	 rptent.fname  -- customer firstname
	,rptent.lname  -- customer last name
	,rptent.entt_id customer_id   -- customer id
    ,rptent.date_of_birth  -- customer date of birt
	,RPT.ContractOid as contractoid  -- contract id
	,rpt.request_date as crtd_datetime  -- created date time  
	,rpt.LastChangeOperator as lupd_user  -- last updated user scorecard -- default is automation when scorecard ran by Aspire
	,rpt.LastChangeDateTime as lupd_datetime  -- last updated date time
	
	,rpt.oid as creditoid  -- credit oid 
	--- format varchar(max) into 3 in order to have valid becon format. 
	,CASE rpt.bureau_oid 
		
            WHEN 4 THEN CAST(ISNULL(Rpt.xml_report.query('data(/XML_INTERFACE/CREDITREPORT/OBJECTS/CCSUBJECT/CCSCORES/ITEM_SCORE/SCORE)'), '') AS VARCHAR(max))
         
			END AS   Beacon   -- parse beacon from customer bureau  type beacon 4

	FROM [dbo].CreditReport (nolock)Rpt   --- credit report table
		LEFT JOIN [dbo].Entity (nolock) RptEnt   			ON Rpt.entt_oid = RptEnt.oid   -- join customer table
     	LEFT JOIN [dbo].CreditBureau (nolock) CB   			on Rpt.Bureau_oid = CB.Oid  -- join credit bureau table in order to ensure we have beacon 4

where rptent.entt_id = '{0}'
order by lupd_datetime desc