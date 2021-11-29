select register_code,last_name,first_name,max(br.name) brname,sum(lottery_tickets) lottery_tickets
from 
(
select a.cust_Code,a.brch_code,b.LAST_NAME,b.first_name,register_code,trunc(case when prod_code='DP22140200' then bal_mnt/50000 else bal_mnt/100000 end) lottery_tickets
from 
(
--3.1  2021/11/01 -с  хойш буюу :date1 -с шинээр нээсэн дансд 
select '2021/11/01 -с  хойш буюу :date1 -с шинээр нээсэн дансд хүүхдийн хадгаламжаас бусад' FK,b.bal_date,b.brch_code,c.cust_code,b.acnt_code,c.name,b.seg_code,b.cur_Code,b.crnt*rate BAL_MNT,trunc(decode(c.CREATED_DATE,'',c.CREATED_DATETIME,c.CREATED_DATE)) CREATED_DATE,
c.start_date,MATURITY_DATE end_date,b.prod_code,gl,bp.name prod_name
FROM NES_WH.TD_ACCOUNT_BAL  B 
left join NES_WH.TD_ACCOUNT_key  bb on b.bal_date=bb.txn_date and bb.ACNT_CODE=b.ACNT_CODE
left join NES_WH.TD_ACCOUNT c on c.modif_date=bb.modif_date and c.ACNT_CODE=bb.ACNT_CODE 
left join nes_wh.glip_gl_cfg_detail p on p.prod_code=b.prod_code
left join nes_wh.bcom_prod bp on bp.PROD_CODE = B.PROD_CODE
where b.bal_date=:date2 and b.bal_day=extract(day from :date2)
and p.bal_type_code='CRNT' and p.prod_type='TD'
and p.gl not like '222%'
and b.seg_code in ('81','52')
and trunc(decode(c.CREATED_DATE,'',c.CREATED_DATETIME,c.CREATED_DATE))>=:date1
--and b.prod_code<>'DP22140200'
--and  trunc((c.maturity_date -case when c.start_Date>'1-nov-21' then c.start_date else trunc(Last_Day(ADD_MONTHS(sysdate,-1))+1) end )/30.3)>=3 
and c.maturity_date>=:date2
and c.tenor>=90
union all
--3.3  3сар буюу түүнээс дээш хугацаатай хадгаламжиндаа орлого хийсэн 
--/11/01-с өмнө нээгдсэн хадгаламж 11/01-с хойш үлдсэн хугацаа нь 3 сар ба түүнээс дээш, дансандаа орлого хийсэн/
select '/11/01-с өмнө нээгдсэн хадгаламж 11/01-с хойш үлдсэн хугацаа нь 3 сар ба түүнээс дээш, дансандаа орлого хийсэн/' FK ,b.bal_date,b.brch_code,cust_code,b.acnt_code,c.name,b.seg_code,b.cur_Code,
--(b.crnt*b.rate)-(b1.crnt*b1.rate) BAL_MNT,
case when (b.crnt*b.rate)-(b1.crnt*b1.rate)<=0 then (b.crnt*b.rate) else (b.crnt*b.rate)-(b1.crnt*b1.rate) end BAL_MNT,
trunc(decode(c.CREATED_DATE,'',c.CREATED_DATETIME,c.CREATED_DATE)) CREATED_DATE,
c.start_date,MATURITY_DATE end_date,b.prod_code,gl,bp.name prod_name
FROM NES_WH.TD_ACCOUNT_BAL  B 
left join NES_WH.TD_ACCOUNT_key  bb on b.bal_date=bb.txn_date and bb.ACNT_CODE=b.ACNT_CODE
left join NES_WH.TD_ACCOUNT c on c.modif_date=bb.modif_date and c.ACNT_CODE=bb.ACNT_CODE 
left join nes_wh.glip_gl_cfg_detail p on p.prod_code=b.prod_code
left join nes_wh.bcom_prod bp on bp.PROD_CODE = B.PROD_CODE
left join nes_wh.td_Account_bal b1 on b1.ACNT_CODE = b.acnt_Code and b1.bal_date=:date1-1 and b1.bal_day=extract(day from :date1-1)
where b.bal_date=:date2 and b.bal_day=extract(day from :date2)
and p.bal_type_code='CRNT' and p.prod_type='TD'
and p.gl not like '222%'
and b.seg_code in ('81','52')
and trunc(decode(c.CREATED_DATE,'',c.CREATED_DATETIME,c.CREATED_DATE))<'1-nov-21'
--and b.prod_code<>'DP22140200'
--and  trunc((c.maturity_date -case when c.start_Date>'1-nov-21' then c.start_date else trunc(Last_Day(ADD_MONTHS(sysdate,-1))+1) end )/30.3)>=3 
and c.maturity_date>=:date2
and c.tenor>=90
and (b.crnt*b.rate)-(b1.crnt*b1.rate)<>0
and b.acnt_code not in (
--:date1 -c өмнө хугацаагүй байсан :date2-т хугацаатай болсон хадгаламж
select a.acnt_code 
from nes_wh.td_account_bal a
left join NES_WH.TD_ACCOUNT_key  b on a.bal_date=b.txn_date and a.ACNT_CODE=b.ACNT_CODE
left join NES_WH.TD_ACCOUNT c on c.modif_date=b.modif_date and c.ACNT_CODE=b.ACNT_CODE 
left join nes_wh.glip_gl_cfg_detail p on p.prod_code=a.prod_code
inner join nes_wh.td_Account_bal b1 on b1.acnt_code=a.acnt_code and b1.bal_date=:date1-1
where a.bal_date=:date2 and a.bal_day=extract(day from :date2) and p.bal_type_code='CRNT' and p.prod_type='TD'
and b1.prod_code='DP22126000' and a.prod_Code<>'DP22126000'
and a.seg_Code in ('81','52') 
--and trunc((c.maturity_date -case when c.start_Date>'1-nov-21' then c.start_date else trunc(Last_Day(ADD_MONTHS(sysdate,-1))+1) end )/30.3)>=3 
and c.maturity_date>=:date2
and c.tenor>=90
union all
--:date1 -т хугацаагүй хадгаламж байсан :date2 -т хугаца уртассан хадгаламжид
select a.acnt_code  
from (
select a.bal_date,a.brch_code,a.acnt_code,c.name,a.seg_code,a.cur_Code,(a.crnt*a.rate) BAL_MNT,
trunc(decode(CREATED_DATE,'',c.CREATED_DATETIME,CREATED_DATE)) CREATED_DATE,
c.start_date,MATURITY_DATE end_date,a.prod_code,gl,bp.name prod_name,c.cust_Code,tenor
FROM NES_WH.TD_ACCOUNT_BAL  a 
left join NES_WH.TD_ACCOUNT_key  b on a.bal_date=b.txn_date and a.ACNT_CODE=b.ACNT_CODE
left join NES_WH.TD_ACCOUNT c on c.modif_date=b.modif_date and c.ACNT_CODE=b.ACNT_CODE 
left join nes_wh.glip_gl_cfg_detail p on p.prod_code=a.prod_code
where a.bal_date=:date2 and a.bal_day=extract(day from :date2) and p.bal_type_code='CRNT' and p.prod_type='TD'
and a.prod_code<>'DP22126000' 
and a.seg_Code in ('81','52')
) a
inner join (
select b.*,c.START_DATE,c.MATURITY_DATE  
FROM NES_WH.TD_ACCOUNT_BAL  B 
left join NES_WH.TD_ACCOUNT_key  bb on b.bal_date=bb.txn_date and bb.ACNT_CODE=b.ACNT_CODE
left join NES_WH.TD_ACCOUNT c on c.modif_date=bb.modif_date and c.ACNT_CODE=bb.ACNT_CODE 
where b.bal_date=:date1-1 and b.bal_day=extract(day from :date1-1)
and b.prod_code<>'DP22126000' 
and b.seg_Code in ('81','52')
) b on b.acnt_code=a.acnt_code and b.bal_date=:date1-1
where a.end_date > b.MATURITY_DATE 
--and trunc((a.end_date -case when a.start_Date>'1-nov-21' then a.start_date else trunc(Last_Day(ADD_MONTHS(sysdate,-1))+1) end )/30.3)>=3 
and a.end_date>=:date2
and a.tenor>=90
)
--хугацаа сунгасан хадгаламжийн дансдыг орлого хийсэн дансдаас хасаж үлдэгдлээр сугалааны тоог авах 
union all
--3.2 аяны хугацаанд хугацаа сунгасан тохиолдолд 

--:date1 -c өмнө хугацаагүй байсан :date2-т хугацаатай болсон хадгаламж
select ':date1 -c өмнө хугацаагүй байсан :date2-т хугацаатай болсон хадгаламж' FK ,a.bal_date,a.brch_code,cust_code,a.acnt_code,c.name,a.seg_code,a.cur_Code,(a.crnt*a.rate) BAL_MNT,
trunc(decode(CREATED_DATE,'',c.CREATED_DATETIME,CREATED_DATE)) CREATED_DATE,
c.start_date,MATURITY_DATE end_date,a.prod_code,gl,bp.name prod_name  
from nes_wh.td_account_bal a
left join NES_WH.TD_ACCOUNT_key  b on a.bal_date=b.txn_date and a.ACNT_CODE=b.ACNT_CODE
left join NES_WH.TD_ACCOUNT c on c.modif_date=b.modif_date and c.ACNT_CODE=b.ACNT_CODE 
left join nes_wh.glip_gl_cfg_detail p on p.prod_code=a.prod_code
left join nes_wh.bcom_prod bp on bp.PROD_CODE = a.PROD_CODE
inner join nes_wh.td_Account_bal b1 on b1.acnt_code=a.acnt_code and b1.bal_date=:date1-1
where a.bal_date=:date2 and a.bal_day=extract(day from :date2) and p.bal_type_code='CRNT' and p.prod_type='TD'
and b1.prod_code='DP22126000' and a.prod_Code<>'DP22126000'
and a.seg_Code in ('81','52') 
--and trunc((c.maturity_date -case when c.start_Date>'1-nov-21' then c.start_date else trunc(Last_Day(ADD_MONTHS(sysdate,-1))+1) end )/30.3)>=3 
and c.maturity_date>=:date2
and c.tenor>=90

union all
--:date1 -т хугацаагүй хадгаламж байсан :date2 -т хугаца уртассан хадгаламжид
select ':date1 -т хугацаатай хадгаламж байсан :date2 -т хугаца уртассан хадгаламжид' fk,a.bal_date,a.brch_code,a.cust_code,a.acnt_code,a.name,a.seg_code,a.cur_Code,a.BAL_MNT,
a.CREATED_DATE,
a.start_date,a.end_date,a.prod_code,gl,a.prod_name    
from (
select a.bal_date,a.brch_code,a.acnt_code,c.name,a.seg_code,a.cur_Code,(a.crnt*a.rate) BAL_MNT,
trunc(decode(CREATED_DATE,'',c.CREATED_DATETIME,CREATED_DATE)) CREATED_DATE,
c.start_date,MATURITY_DATE end_date,a.prod_code,gl,bp.name prod_name,c.cust_Code,tenor
FROM NES_WH.TD_ACCOUNT_BAL  a 
left join NES_WH.TD_ACCOUNT_key  b on a.bal_date=b.txn_date and a.ACNT_CODE=b.ACNT_CODE
left join NES_WH.TD_ACCOUNT c on c.modif_date=b.modif_date and c.ACNT_CODE=b.ACNT_CODE 
left join nes_wh.glip_gl_cfg_detail p on p.prod_code=a.prod_code
left join nes_wh.bcom_prod bp on bp.PROD_CODE = a.PROD_CODE
where a.bal_date=:date2 and a.bal_day=extract(day from :date2) and p.bal_type_code='CRNT' and p.prod_type='TD'
and a.prod_code<>'DP22126000' 
and a.seg_Code in ('81','52')
) a
inner join (
select b.*,c.START_DATE,c.MATURITY_DATE  
FROM NES_WH.TD_ACCOUNT_BAL  B 
left join NES_WH.TD_ACCOUNT_key  bb on b.bal_date=bb.txn_date and bb.ACNT_CODE=b.ACNT_CODE
left join NES_WH.TD_ACCOUNT c on c.modif_date=bb.modif_date and c.ACNT_CODE=bb.ACNT_CODE 
where b.bal_date=:date1-1 and b.bal_day=extract(day from :date1-1)
and b.prod_code<>'DP22126000' 
and b.seg_Code in ('81','52')
) b on b.acnt_code=a.acnt_code and b.bal_date=:date1-1
where a.end_date > b.MATURITY_DATE 
--and trunc((a.end_date -case when a.start_Date>'1-nov-21' then a.start_date else trunc(Last_Day(ADD_MONTHS(sysdate,-1))+1) end )/30.3)>=3 
and a.end_date>=:date2
and a.tenor>=90
)a
left join nes.cif_person@pdb@sdblink b on b.cust_code=a.cust_code

where bal_mnt>=50000 and a.cust_code not in (select cust_code from purevbayar.ST_HRUSERS)  --төрийн банкны ажилтны сиф дугаар хасах
)a
left join nes.gen_branch@pdb@sdblink br on br.brch_code=a.brch_code
where lottery_tickets>0 
--and register_code=trim(upper(:id1))
group by cust_Code,last_name,first_name,register_code

/*2021/11/6 өдөр сугалааны эрхийн тоог авах үүсгэв*/
/*2021/11/12 дахин засварлав*/