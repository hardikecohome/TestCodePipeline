﻿SELECT top 1 e.entt_id,e.oid, e.name, e.fname, e.lname, e.date_of_birth, e.email_addr, 
                                                 l.postal_code, l.city, l.state, l.postal_code, l.addr_line1,
                                                 main.phone_num AS main_phone, fax.phone_num as Fax
FROM Entity (nolock) as e
INNER JOIN SecurityUser (nolock) sc on sc.oid = e.secu_oid
LEFT JOIN Location (nolock) as l
    on (e.oid = l.entt_oid and e.loca_oid = l.oid)
LEFT JOIN Phone (nolock) as main
    on (e.oid = main.entt_oid and main.phone_type ='M')
LEFT JOIN Phone (nolock) as fax
    on (e.oid = fax.entt_oid and fax.phone_type ='F')
where sc.user_id  LIKE '{0}%';