SELECT   cast(e.status_date as date) as status_date, 
d_type.value as Product_Type,
c_type.value as Channel_Type,
ratecard.value as ratecard,
rol.active,
rol.inactive_date,
--rol.role_type,
--rol.system_defined,
rol.descr as Role,
sc.user_id,
sc.UpdateCount,
sc.LastChangeDateTime,
sc.LastChangeOperator,
sc.change_password,
sc.password_exp_date,
sc.deactivation_date,
sc.contract_attach_role_oid,
sc.display_tooltips,

e.entt_id,e.oid, COALESCE(p.name,e.name) as name ,COALESCE(p.name,COALESCE(e.name,e.fname)) as fname, e.lname, e.date_of_birth, e.email_addr, 
                                                 l.postal_code, l.city, l.state, l.addr_line1,
                                                 main.phone_num AS phone_num, fax.phone_num as Fax,
												 p.name as parent_name,
												 scparent.user_id as parent_uname
FROM Entity (nolock) as e
INNER JOIN SecurityUser (nolock) sc on sc.oid = e.secu_oid
LEFT JOIN SecurityUser (nolock) scparent on scparent.oid = e.parent_oid
LEFT JOIN Location (nolock) as l
    on (e.oid = l.entt_oid and e.loca_oid = l.oid)
LEFT JOIN Phone (nolock) as main
    on (e.oid = main.entt_oid and main.phone_type ='M')
LEFT JOIN Phone (nolock) as fax
    on (e.oid = fax.entt_oid and fax.phone_type ='F')
 LEFT JOIN Entity (nolock) as p 	on e.parent_oid = p.oid  
 LEFT JOIN [dbo].[Role] (NOLOCK) AS rol on rol.[oid] = sc.contract_attach_role_oid
 LEFT JOIN [DocGenAccOtherUDF-Dealer Type] d_type (NOLOCK) ON e.oid = d_type.oid
 LEFT JOIN [DocGenAccOtherUDF-ChannelType] c_type (NOLOCK) ON e.oid = c_type.oid
 LEFT JOIN [DocGenAccOtherUDF-RateCard] ratecard (NOLOCK) ON e.oid = ratecard.oid

 where sc.user_id  LIKE '{0}%'
 
order by 1 desc